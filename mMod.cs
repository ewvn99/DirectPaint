global using MathF = System.MathF;
using Cursor = System.Windows.Forms.Cursor;
using System.Diagnostics.CodeAnalysis;
using Wew;
using Wew.Control;
using Wew.Media;

namespace DirectPaint;

enum eHerr																// Herramientas con estado
{	Sel			= 0,
	SelLibre,
	Mano,
	Texto,
	Muestra,
	Pincel,
	Aerosol,
	Flood,
	Rect,
	RectRedond,
	Elipse,
	Lin,
	Poli,
	PoliLin,
	Arc,
	Bezier,
	QuadraticBezier,
	Glifo,
	Iluminar,
	Blur
}
abstract class cPrimitivo : Wew.Control.cUndo.cItem						// ** Sólo se descartan los recursos no compartidos: geoms y bmps
{	readonly Point m_ptTamAntImg, m_ptTamImg;
	readonly bool m_bBorrarPred; bool m_bBorrar;
	bool m_bAntialias;
	readonly cLineStyle? m_lisTrazoPred;
	protected bool _PintarBorde, _Rellenar;
	protected cBrush _Borde, _Relleno;
	protected cLineStyle _Trazo; protected float _AnchoLin;
	protected Rectangle m_rtLims;
	public readonly fVista Vista;
	public Matrix3x2? Transform;
	public int CantSubDibu;												// Sólo para curvas (ej: pincel, no geom)
	protected cPrimitivo(fVista visPad, bool bBorrar = false, cLineStyle? lisTrazo = null, int iTamBlob = 0) : base(iTamBlob) // Tam en bytes: sólo para bmp; en caso de mdl3d se establece al asig mdl
	{	Vista = visPad;
		m_ptTamAntImg = visPad.TamAnt; m_ptTamImg = visPad.Tam;
		m_bBorrarPred = bBorrar;
		m_lisTrazoPred = lisTrazo;
		m_AplicaCfg();
	}
	public Point TamAntImg							=>	m_ptTamAntImg;
	public Point TamImg								=>	m_ptTamImg;
	public Rectangle Lims							=>	m_rtLims;
	public virtual void OnCfgChanged()				{	m_AplicaCfg(); Vista.Changed = true;}
	public abstract void OnMouseDrag(Point ptPt);
	public virtual void OnMouseUp()					{}
	public virtual void OnPresCtrl(Point ptPt)		{}
	public virtual void Pinta(cGraphics g)
	{	Matrix3x2? m32 = null;

		// ** Borrar
		if (m_bBorrar)	g.PrimitiveBlend = ePrimitiveBlend.Copy;
		// ** Pintar
		g.Antialias = m_bAntialias;
		if (Transform is not null)	{	m32 = g.Transform; g.Transform = Transform.Value * g.Transform;}
		_Pinta(g);
		// ** Restablecer
		if (m32 is not null)	{	g.Transform = m32.Value;}
		g.PrimitiveBlend = ePrimitiveBlend.Default;
	}
	protected abstract void _Pinta(cGraphics g);
		[MemberNotNull(nameof(_Borde), nameof(_Relleno), nameof(_Trazo))]
	void m_AplicaCfg()
	{	m_bBorrar = (m_bBorrarPred | mMod.Herrs.Borrar);
		if (m_bBorrar)													// ** Borrar: reemplazar con fondo
		{	_Borde = _Relleno = Vista.Fondo;
			_PintarBorde = false; _Rellenar = true;
		} else															// ** Pintar normalmente
		{	_Borde = (this is cPrimTxt ? mMod.Colores.BrFuente : mMod.Colores.Borde);
			_Relleno = mMod.Colores.Relleno;
			_PintarBorde = mMod.Herrs.Borde; _Rellenar = mMod.Herrs.Rellenar;
		}
		m_bAntialias = mMod.Cfg.Antialias;
		_Trazo = (m_lisTrazoPred ?? mMod.Cfg.Trazo);
		_AnchoLin = mMod.Cfg.AnchoLin;
	}
}
sealed class cPrimPincel : cPrimitivo
{	static readonly cLineStyle s_lsTrazo = new (eDash.Solid, eCapStyle.Round, eLineJoin.Round);
	readonly Point m_ptIni; Array<Point> ma_ptPts;
	public cPrimPincel(fVista visPad, Point ptIni) : base(visPad, lisTrazo: s_lsTrazo) // Usar trazo propio
	{	m_ptIni = ptIni; ma_ptPts.MinimumCapacity = 100;
	}
	public override void OnMouseDrag(Point ptPt)			{	ma_ptPts.Add(ptPt); CantSubDibu = ma_ptPts.Count; Vista.AddDibu();}
	protected override void _Pinta(cGraphics g)
	{	Point ptAnt, pt;

		ptAnt = m_ptIni;
		for (int i = 0, iFin = ma_ptPts.Count; i < iFin; i++)
		{	pt = ma_ptPts[i];
			g.DrawLine(ptAnt, pt, _Borde, _AnchoLin, _Trazo);
			ptAnt = pt;
		}
	}
}
sealed class cPrimLin(fVista visPad, Point ptIni) : cPrimitivo(visPad)
{	readonly Point m_ptIni = ptIni; Point m_ptFin = ptIni;
	bool m_bSnap = mMod.Cfg.SnapLin;
	public override void OnCfgChanged()				{	base.OnCfgChanged(); m_bSnap = mMod.Cfg.SnapLin;}
	public override void OnMouseDrag(Point ptPt)	{	m_ptFin = ptPt;}
	public override void OnMouseUp()				{	m_rtLims = (Rectangle)new Rect(m_ptIni, m_ptFin).GetSortedSides();}
	protected override void _Pinta(cGraphics g)
	{	g.DrawLine(m_ptIni, m_ptFin, _Borde, _AnchoLin, _Trazo, m_bSnap);
	}
}
sealed class cPrimRect : cPrimitivo
{	Rectangle m_rtRect;
	public cPrimRect(fVista visPad, Point ptIni) : base(visPad)	{	m_rtRect.Location = ptIni;}
	public cPrimRect(fVista visPad, Rectangle rtPlace) : base(visPad, true)	{	m_rtRect = rtPlace;} // Para borrar sel
	public override void OnMouseDrag(Point ptPt)	{	m_rtRect.Size = ptPt - m_rtRect.Location;}
	public override void OnMouseUp()				{	m_rtRect.SortSides(); m_rtLims = m_rtRect;}
	protected override void _Pinta(cGraphics g)
	{	if (_Rellenar)		g.FillRectangle(m_rtRect, _Relleno, false);
		if (_PintarBorde)	g.DrawRectangle(m_rtRect, _Borde, _AnchoLin, _Trazo, false);
	}
}
sealed class cPrimElip : cPrimitivo
{	Rectangle m_rtRect;
	public cPrimElip(fVista visPad, Point ptIni) : base(visPad)	{	m_rtRect.Location = ptIni;}
	public cPrimElip(fVista visPad, Rectangle rtPlace) : base(visPad, true)	{	m_rtRect = rtPlace;} // Para borrar sel
	public override void OnMouseDrag(Point ptPt)	{	m_rtRect.Size = ptPt - m_rtRect.Location;}
	public override void OnMouseUp()				{	m_rtRect.SortSides(); m_rtLims = m_rtRect;}
	protected override void _Pinta(cGraphics g)
	{	if (_Rellenar)		g.FillEllipse(m_rtRect, _Relleno);
		if (_PintarBorde)	g.DrawEllipse(m_rtRect, _Borde, _AnchoLin, _Trazo);
	}
}
sealed class cPrimRectRedond : cPrimitivo
{	RoundedRectangle m_rrRect;
	public cPrimRectRedond(fVista visPad, Point ptIni) : base(visPad)	{	m_rrRect.Location = ptIni; m_rrRect.Radius = mMod.Cfg.RrEsquina;}
	public override void OnCfgChanged()				{	base.OnCfgChanged(); m_rrRect.Radius = mMod.Cfg.RrEsquina;}
	public override void OnMouseDrag(Point ptPt)	{	m_rrRect.Size = ptPt - m_rrRect.Location;}
	public override void OnMouseUp()				{	m_rrRect.Rectangle.SortSides(); m_rtLims = m_rrRect.Rectangle;}
	protected override void _Pinta(cGraphics g)
	{	if (_Rellenar)		g.FillRoundedRectangle(m_rrRect, _Relleno);
		if (_PintarBorde)	g.DrawRoundedRectangle(m_rrRect, _Borde, _AnchoLin, _Trazo);
	}
}
sealed class cPrimBmp : cPrimitivo											// ** El bmp no debe ser compartido porque se descartará con Dispose()
{	readonly cBitmap m_bmpBitmap;
	readonly Rectangle m_rtRect;
	public cPrimBmp(fVista visPad, cBitmap bmpBitmap)
		: base(visPad, iTamBlob: bmpBitmap.CalculateLineSize() * bmpBitmap.PixelSize.Y)
	{	m_bmpBitmap = bmpBitmap; m_rtRect.Size = TamImg;					// Dibujar en todo el fondo: es el bitmap del archivo
	}
	public cPrimBmp(fVista visPad, cBitmap bmpBitmap, Rectangle rtRect)
		: base(visPad, iTamBlob: bmpBitmap.CalculateLineSize() * bmpBitmap.PixelSize.Y)
	{	m_bmpBitmap = bmpBitmap; m_rtRect = rtRect;							// Dibujar sobre la sel
	}
	protected override void Dispose(bool disposing)	{	m_bmpBitmap.Dispose(); base.Dispose(disposing);}
	public override void OnMouseDrag(Point ptPt)	{}
	public override void OnMouseUp()				{	m_rtLims = m_rtRect;}
	protected override void _Pinta(cGraphics g)
	{	g.DrawBitmap(m_bmpBitmap, m_rtRect, eAlignment.None, eResize.Stretch, 1, eInterpolation.Linear);
	}
}
sealed class cPrimTxt(fVista visPad, Rectangle rtRect, string sTexto) : cPrimitivo(visPad)
{	readonly Rectangle m_rtRect = rtRect;
	readonly string m_sTexto = sTexto; readonly cFont m_fntFnt = mMod.Colores.Fuente;
	public override void OnMouseDrag(Point ptPt)			{}
	protected override void _Pinta(cGraphics g)				{	g.DrawText(m_sTexto, m_rtRect, _Borde, m_fntFnt, eTextFormat.Default);}
}
sealed class cPrimCmdLs : cPrimitivo												// Junta todos los dibujos a un cmdls y lo asigna (input) a un ef
{	/*readonly*/ cGpuCommandList m_cmlList; readonly cEffect m_efEf;
	public cPrimCmdLs(fVista visPad, cEffect efEf) : base(visPad)	{	m_efEf = efEf; Compacta(visPad.GetUndo());}
	protected override void Dispose(bool disposing)			{	m_cmlList.Dispose(); base.Dispose(disposing);}
	public cEffect Efecto									=>	m_efEf;
	public override void OnMouseDrag(Point ptPt)			{}
	public override void Pinta(cGraphics g)					{	g.DrawImage(m_efEf, default, eInterpolation.Linear);}//¿, eComposite.Xor);}
		[MemberNotNull(nameof(m_cmlList))]
	public void Compacta(cUndo undUndo, int iPrimIni = -2, int iPrimFin = -2) // Recrea el cmdls a partir del nuevo prim ini o cmdls base
	{	m_cmlList?.Dispose();												// ** Crear ls con todos los dibujos (desde un prim ini o un cmdls)
			m_cmlList = new cGpuCommandList(mMod.MainWnd);
			m_cmlList.Add((cGraphics g) => undUndo.Pinta(g, iPrimIni, iPrimFin)); m_cmlList.EndList();
		if (m_efEf is cSingleInputEffect sie)								// ** Asig input (ls) a ef
			sie.Input = m_cmlList;
		else if (m_efEf is cDualInputEffect die)
			die.Destination = m_cmlList;
		else if (m_efEf is cCompositeEffect cef)
			cef[0] = m_cmlList;
	}
	protected override void _Pinta(cGraphics g)				{}
}
abstract class cPrimGeometría : cPrimitivo							// ** La geom no debe ser compartida porque se descartará con Dispose()
{	cGeometry m_geoGeom; readonly bool m_bGeomEditable = true, m_bBeginFig = true;
	readonly Point m_ptIni; Point m_ptFin; bool m_bCerrado, m_bRellenar;
	protected cPrimGeometría(fVista visPad, Point ptIni, bool bBeginFig = true) : base(visPad)
	{	m_ptIni = m_ptFin = ptIni; m_bCerrado = mMod.Herrs.Cerrado;
		m_geoGeom = new cPathGeometry((cPathGeometry.Sink s) => {}); m_bBeginFig = bBeginFig;
	}
	protected cPrimGeometría(fVista visPad, cGeometry geoGeom, bool bBorrar = false) : base(visPad, bBorrar) // Para borrar sel
	{	m_geoGeom = geoGeom; m_bGeomEditable = true;
	}
	protected override void Dispose(bool disposing)
	{	m_geoGeom?.Dispose(); m_geoGeom = null!;
		base.Dispose(disposing);
	}
	public override void OnCfgChanged()
	{	base.OnCfgChanged();
		if (b_RecrearFig() | _Rellenar != m_bRellenar | mMod.Herrs.Cerrado != m_bCerrado)
		{	m_bRellenar = _Rellenar;
			m_bCerrado = mMod.Herrs.Cerrado;
			m_CreaFig(m_ptFin, false);
		}
	}
	public sealed override void OnMouseDrag(Point ptPt)	{	m_ptFin = ptPt; m_CreaFig(ptPt, true);}
	public sealed override void OnMouseUp()			{	m_rtLims = m_geoGeom.GetBounds();}
	protected abstract void _LlenaFigura(cPathGeometry.Sink pgsSink, Point ptPt, bool bAgregar = true);
	protected virtual bool b_RecrearFig()			{	return false;}
	protected override void _Pinta(cGraphics g)
	{	if (_Rellenar)		g.FillGeometry(m_geoGeom, _Relleno);
		if (_PintarBorde)	g.DrawGeometry(m_geoGeom, _Borde, _AnchoLin, _Trazo);
	}
	void m_CreaFig(Point ptPt, bool bAgregar = true)
	{		if (!m_bGeomEditable)	return;							// No recrear
		cPathGeometry pg;
	
		pg = new cPathGeometry(										// ** Crear nueva figura
			(cPathGeometry.Sink pgsSink) =>
			{	if (m_bBeginFig)	pgsSink.BeginFigure(m_ptIni, _Rellenar);
				_LlenaFigura(pgsSink, ptPt, bAgregar);
				if (m_bBeginFig)	pgsSink.EndFigure(m_bCerrado);
		    });
		m_geoGeom.Dispose(); m_geoGeom = pg;
	}
}
sealed class cGeomArc(fVista visPad, Point ptIni) : cPrimGeometría(visPad, ptIni)
{	float m_fAng = mMod.Cfg.ArcAng; Point m_ptRadio = mMod.Cfg.ArcRadio;
	bool m_bContrarrel = mMod.Cfg.ArcContrarrel, m_bPeque = mMod.Cfg.ArcPeque;
	protected override bool b_RecrearFig()
	{	if (mMod.Cfg.ArcAng != m_fAng | mMod.Cfg.ArcRadio != m_ptRadio
				| mMod.Cfg.ArcContrarrel != m_bContrarrel | mMod.Cfg.ArcPeque != m_bPeque)
		{	m_fAng = mMod.Cfg.ArcAng; m_ptRadio = mMod.Cfg.ArcRadio;
			m_bContrarrel = mMod.Cfg.ArcContrarrel; m_bPeque = mMod.Cfg.ArcPeque;
			return true;
		}
		return false;
	}
	protected override void _LlenaFigura(cPathGeometry.Sink pgsSink, Point ptPt, bool bAgregar = true)
	{	pgsSink.AddArc(ptPt, m_ptRadio, m_fAng, m_bContrarrel, m_bPeque);
	}
}
sealed class cGeomBezier : cPrimGeometría
{	int m_iCantPts; Point m_pt0, m_pt1;
	public cGeomBezier(fVista visPad, Point ptIni) : base(visPad, ptIni)	{	m_pt0 = m_pt1 = ptIni;}
	public override void OnPresCtrl(Point ptPt)								// Agregar pt
	{	if (m_iCantPts == 0)
		{	m_pt0 = m_pt1 = ptPt; m_iCantPts++;
		} else if (m_iCantPts == 1)
		{	m_pt1 = ptPt; m_iCantPts++;
		}
	}
	protected override void _LlenaFigura(cPathGeometry.Sink pgsSink, Point ptPt, bool bAgregar = true)
	{	pgsSink.AddBezier(m_pt0, m_pt1, ptPt);
	}
}
sealed class cGeomQuadraticBezier(fVista visPad, Point ptIni) : cPrimGeometría(visPad, ptIni)
{	Point m_ptCtl = ptIni; bool m_bTieneCtl;
	public override void OnPresCtrl(Point ptPt)			{	if (!m_bTieneCtl)	{	m_ptCtl = ptPt; m_bTieneCtl = true;}} // Agregar pt
	protected override void _LlenaFigura(cPathGeometry.Sink pgsSink, Point ptPt, bool bAgregar = true)
	{	pgsSink.AddQuadraticBezier(m_ptCtl, ptPt);
	}
}
sealed class cGeomPolígono : cPrimGeometría
{	Array<Point> ma_ptPts = new (100);
	public cGeomPolígono(fVista visPad, Point ptIni) : base(visPad, ptIni)	{}
	public cGeomPolígono(fVista visPad, cGeometry geoBorrar) : base(visPad, geoBorrar, true)	{} // Para borrar
	protected override void _LlenaFigura(cPathGeometry.Sink pgsSink, Point ptPt, bool bAgregar = true)
	{	if (bAgregar)	ma_ptPts.Add(ptPt);
		pgsSink.AddLines(ma_ptPts.Data, 0, ma_ptPts.Count);
	}
}
sealed class cGeomPoliLin(fVista visPad, Point ptIni) : cPrimGeometría(visPad, ptIni)
{	Array<Point> ma_ptPts = new (10);
	public override void OnPresCtrl(Point ptPt)			{	ma_ptPts.Add(ptPt);} // Agregar pt
	protected override void _LlenaFigura(cPathGeometry.Sink pgsSink, Point ptPt, bool bAgregar = true)
	{	pgsSink.AddLines(ma_ptPts.Data, 0, ma_ptPts.Count);
		if (bAgregar)	pgsSink.AddLine(ptPt);
	}
}
sealed class cGeomGlifo(fVista visPad, Point ptIni, cFont fntFnt, short shGlifo) : cPrimGeometría(visPad, ptIni, false)
{	readonly cFont m_fntFnt = fntFnt; readonly short m_shGlifo = shGlifo; readonly float m_fTam = 50;
	protected override void _LlenaFigura(cPathGeometry.Sink pgsSink, Point ptPt, bool bAgregar = true)
	{
		pgsSink.AddGlyphGeometry(m_fntFnt, m_shGlifo, m_fTam);
	}
}
sealed class cPrimObj3D : cPrimitivo
{	c3DModel m_mdlMdl; cBitmap? m_bmpMdl;
	Rectangle m_rtRect, m_rtRectFrag;
	readonly cControl m_ctlLienzo; readonly cCamera m_camCam;
		#pragma warning disable CS8618										// Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de declararlo como que admite un valor NULL.
	public cPrimObj3D(fVista visPad) : base(visPad)			{	m_ctlLienzo = visPad.GetLienzo(out m_camCam);}
		#pragma warning restore CS8618
	protected override void Dispose(bool disposing)
	{	m_mdlMdl?.Dispose(); m_mdlMdl = null!;
		m_bmpMdl?.Dispose(); m_bmpMdl = null;
		base.Dispose(disposing);
	}
	public c3DModel Mdl
	{	set
		{	m_mdlMdl = value; m_camCam.Size = m_ctlLienzo.Size; m_camCam.Frame(m_mdlMdl, Vector.ZAxis, Vector.YAxis);
		}
	}
	public void Pinta3D(PaintArgs e)
	{		if (!m_mdlMdl.Visible)	return;
		c3DGraphics g3d = e.Graphics.Set3DViewport(m_ctlLienzo.AbsoluteBounds, e.ClipRectangle);
		g3d.ClearDepth(); g3d.SetDefaults(m_camCam); g3d.Render(m_mdlMdl);
	}
	public void TerminaEdición()
	{	if (m_mdlMdl.Visible)
		{	m_camCam.Size = (Point)mMod.MainWnd.BackBuffer.Size;			// Cfg mat view
				m_bmpMdl = mMod.MainWnd.Render3DToBitmap((object sender, c3DGraphics g3d) => // ** Pintar mdl y copiar bmp
					{	g3d.ClearTarget(eColor.Transparent);
						g3d.ClearDepth(); g3d.SetDefaults(m_camCam); g3d.Render(m_mdlMdl);
					});
			m_rtRect.Size = (Point)mMod.MainWnd.BackBuffer.Size;//¿
				m_rtRectFrag.Size = Vista.Tam; m_rtRectFrag.Size.ScaleTo(m_rtRect.Size); m_rtRectFrag.CenterIn(m_rtRect);
			m_rtRect.Size = Vista.Tam;
		}
		m_mdlMdl.Dispose(); m_mdlMdl = null!;								// Liberar mdl
	}
	public override void OnMouseDrag(Point ptPt)			{}
	protected override void _Pinta(cGraphics g)
	{		if (m_bmpMdl is null)	return;
		g.DrawBitmap(m_bmpMdl, m_rtRect, m_rtRectFrag, eAlignment.None, eResize.Stretch, 1, eInterpolation.NearestNeighbor);
	}
}
sealed class cUndo : Wew.Control.cUndo
{	readonly fVista m_visPad;
	cPrimitivo? m_priPrimIni; int m_iBase, m_iPrimIni, m_iPrimActual;		// 1° prim a pintar (en caso de Abrir o compactar).  Ini de undos.  1° (-1 = pintar prim ini) y ult primi de rango a pintar
	int m_iCantDibu; readonly int m_iMaxDibus = 1000;						// Cant de dibujos; el tam se considera sólo en el caso de bmps; el tam de los demás dibujos se limita con la cant de dibujos
	static cGpuBitmap? s_gbmSurf;
	public static cGpuBitmap? Surf							=>	s_gbmSurf;
		[MemberNotNull(nameof(s_gbmSurf))] [MemberNotNull(nameof(Surf))]
	public static void RedimSurf(Point ptTam)
	{	s_gbmSurf?.Dispose(); s_gbmSurf = null; s_gbmSurf = new cGpuBitmap(mMod.MainWnd, ptTam.GetRounded());
	}
	public cUndo(fVista visPad)								{	m_visPad = visPad; MaxSize = 50100100; m_iPrimIni = m_iPrimActual = -1;}
	public void SetPrimIni(cPrimitivo priPrim)				{	m_priPrimIni = priPrim;} // Llamado al abrir
	public override void Add(cItem item, bool EndGroup = true)
	{	base.Add(item, EndGroup); m_iCantDibu += 1 + ((cPrimitivo)item).CantSubDibu;
		m_iPrimActual += 1;	if (item is cPrimCmdLs)	m_iPrimIni = m_iPrimActual; // ** Avanzar; si el prim actual es ls: tomarlo como ini de cola
		m_Compacta();														// Compactar (si es necesario)
	}
	public void AddDibu()									{	m_iCantDibu++;}
	public override void Clear()
	{	base.Clear(); m_iPrimIni = m_iPrimActual = -1; m_priPrimIni?.Dispose(); m_priPrimIni = null; m_iCantDibu = 0;
	}
	public void Undo()
	{	cPrimitivo pri; int i;

			if (!CanUndo)	{	m_iPrimIni = m_iPrimActual = -1; return;}	// Sin undo: no pintar rango (sólo prim ini), salir
		pri = (cPrimitivo)this[m_iPrimActual];
		m_iCantDibu -= 1 + pri.CantSubDibu;									// Descontar dibus
		if (pri is cPrimCmdLs)												// ** El prim actual es ls: buscar ls ant
		{	for (i = m_iPrimActual - 1; i >= 0 && this[i] is not cPrimCmdLs; i--)	{}
			m_iPrimIni = i;													// Puede ser -1 (se pintará el prim ini, si existe)
		}
		pri = (cPrimitivo)Undo(out _, out _)!; m_iPrimActual -= 1;			// ** Retroceder
		m_visPad.Tam = pri.TamAntImg; mMod.Cfg.OnVistaEnfocada(m_visPad);	// ** Actualizar tam al ant
	}
	public void Redo()
	{	cPrimitivo pri;

		pri = (cPrimitivo)Redo(out _, out _)!;	if (pri is null)	return;
		m_iPrimActual += 1; if (pri is cPrimCmdLs)	m_iPrimIni = m_iPrimActual; // ** Avanzar; si el prim actual es ls: tomarlo como ini de cola
		m_iCantDibu += 1 + pri.CantSubDibu;									// Contar dibus
		m_visPad.Tam = pri.TamImg; mMod.Cfg.OnVistaEnfocada(m_visPad);		// ** Actualizar tam al actual
	}
	public void Pinta(cGraphics g, int iPrimIni = -2, int iPrimFin = -2)
	{	// ** Fondo: limpiar
		g.PrimitiveBlend = ePrimitiveBlend.Copy;
			g.FillRectangle(new Rectangle(default, m_visPad.Tam), m_visPad.Fondo);
		g.PrimitiveBlend = ePrimitiveBlend.Default;
		// ** Tomar rango pred (el actual)
		if (iPrimIni == -2)	iPrimIni = m_iPrimIni;
		if (iPrimFin == -2)	iPrimFin = m_iPrimActual;
		// ** Pintar todo: pintar pri ini (si hay)
		if (iPrimIni == -1)	{	m_priPrimIni?.Pinta(g); iPrimIni = m_iBase;}
		// ** Primis de rango
		for (; iPrimIni <= iPrimFin; iPrimIni++)	((cPrimitivo)this[iPrimIni]).Pinta(g);
	}
	public void PintaSinEscala(cGraphics g, Point ptDesp, int iPrimIni = -2, int iPrimFin = -2) // Pinta en mem, sin escala (en pix)
	{	g.Transform = Matrix3x2.FromTranslation(ptDesp) * Matrix3x2.FromScale(cGraphics.PixelToDip); // Dibujar 1:1
		Pinta(g, iPrimIni, iPrimFin);
	}
	public cBitmap Copia(Rectangle rtSel = default, cGeometry? geoSelLibre = null, int iPrimIni = -2, int iPrimFin = -2)
	{	if (rtSel.IsEmpty)	rtSel.Size = m_visPad.Tam;						// Sin zona: tomar todo
		// ** Redim
		Point ptTamSurf = (s_gbmSurf?.Size).GetValueOrDefault();
		if (rtSel.Width > ptTamSurf.X || rtSel.Height > ptTamSurf.Y || s_gbmSurf is null // El tam pedido es mayor que la superf (agrandar)
				|| m_visPad.Tam.X < ptTamSurf.X / 3 || m_visPad.Tam.Y < ptTamSurf.Y / 3) // o el tam tot es menor que un tercio de la superf (reducir para ahorrar mem)
		{	RedimSurf(rtSel.Size);
		}
		// ** Pintar
		mMod.MainWnd.Render2DTo(s_gbmSurf, (cGraphics g) =>
			{	PintaSinEscala(g, -rtSel.Location, iPrimIni, iPrimFin);		// Pintar
				if (geoSelLibre is not null)								// ** Despejar sel libre
				{	using cGeometry geoSelInv = m_visPad.InvSel(geoSelLibre);

					g.Antialias = true; g.PrimitiveBlend = ePrimitiveBlend.Copy; // Borrar todo menos la sel
					g.FillGeometry(geoSelInv, eBrush.Transparent);
				}
			});
		// ** Copiar
		return s_gbmSurf.ToBitmap(new RectangleI(default, (PointI)rtSel.Size));
	}
	protected override void OnRemovingItems(int index, int count, bool compacting)
	{	if (compacting)														// ** Compactar
		{	int iIniRango, iFinRango; cPrimitivo pri;

			// ** Crear bmp
			for (iFinRango = index + count - 1, iIniRango = iFinRango; iIniRango >= 0 && this[iIniRango] is not cPrimCmdLs; iIniRango--)	{} // Buscar cmdls ant (o prim ini) para empezar a pintar nuevo bmp
			pri = new cPrimBmp(m_visPad, Copia(new Rectangle(default, ((cPrimitivo)this[iFinRango]).TamImg), null, iIniRango, iFinRango));
			m_priPrimIni?.Dispose(); m_priPrimIni = pri;
			// ** Compactar cmdlss hasta el final
			for (iFinRango++, m_iBase = iFinRango, m_iCantDibu = 0; iFinRango < this.Count; iFinRango++)
			{	pri = (cPrimitivo)this[iFinRango]; m_iCantDibu += 1 + pri.CantSubDibu;	if (pri is not cPrimCmdLs)	continue; // No es cmdls: omitir
				((cPrimCmdLs)this[iFinRango]).Compacta(this, iIniRango, iFinRango - 1); iIniRango = iFinRango;
			}
			// ** Actualizar rango
			for (m_iPrimIni = m_iPrimActual; m_iPrimIni >= m_iBase && this[m_iPrimIni] is not cPrimCmdLs; m_iPrimIni--)	{} // Buscar cmdls ant (o prim ini) para empezar a pintar
			if (m_iPrimIni < m_iBase)	m_iPrimIni = -1;	else	m_iPrimIni -= count;
			m_iPrimActual -= count; m_iBase = 0;
		}
		base.OnRemovingItems(index, count, compacting);
	}
	private void m_Compacta()
	{	int iExceso, iFin, i;

		iExceso = m_iCantDibu - m_iMaxDibus;	if (iExceso <= m_iMaxDibus * 0.33f)	return; // Pocos dibus o hay tolerancia: salir
		for (i = 0, iFin = GetGroupStart(); i < iFin && iExceso > 0; i++)	iExceso -= 1 + ((cPrimitivo)this[i]).CantSubDibu; // Buscar exceso antes de la pos actual
		Compact(i); m_iCantDibu = m_iMaxDibus + iExceso;						// ** Compactar
	}
}
static class eCmds
{	public static cCommand EfNuevo = new ("New effect", mRes.BmpNuevoEf);
}
static class mRes
{	public static readonly cBitmap BmpCur = new (typeof(mRes), "Res.Cur.png");
	public static readonly cBitmap BmpFrames = new (typeof(mRes), "Res.Frames.png");
	public static readonly cBitmap BmpHerr = new (typeof(mRes), "Res.Herr.png");
	public static readonly cBitmap BmpBrush = new (typeof(mRes), "Res.Brush.png");
	public static readonly cBitmap BmpEfecto = new (typeof(mRes), "Res.Efecto.png");
	public static readonly cBitmap BmpCfg = new (typeof(mRes), "Res.Cfg.png");
	public static readonly cBitmap BmpNuevoEf = new (typeof(mRes), "Res.NuevoEf.png");
	public static readonly cBitmap BmpFondoTransp = new (typeof(mRes), "Res.FondoTransp.png");
	public static readonly cBitmap BmpSel = new (typeof(mRes), "Res.Sel.png");
	public static readonly cBitmap BmpSelLibre = new (typeof(mRes), "Res.SelLibre.png");
	public static readonly cBitmap BmpInvSel = new (typeof(mRes), "Res.InvSel.png");
	public static readonly cBitmap BmpMano = new (typeof(mRes), "Res.Mano.png");
	public static readonly cBitmap BmpTexto = new (typeof(mRes), "Res.Texto.png");
	public static readonly cBitmap BmpMuestra = new (typeof(mRes), "Res.Muestra.png");
	public static readonly cBitmap BmpPincel = new (typeof(mRes), "Res.Pincel.png");
	public static readonly cBitmap BmpAerosol = new (typeof(mRes), "Res.Aerosol.png");
	public static readonly cBitmap BmpFlood = new (typeof(mRes), "Res.Flood.png");
	public static readonly cBitmap BmpRect = new (typeof(mRes), "Res.Rect.png");
	public static readonly cBitmap BmpElipse = new (typeof(mRes), "Res.Elipse.png");
	public static readonly cBitmap BmpRectRedond = new (typeof(mRes), "Res.RectRedond.png");
	public static readonly cBitmap BmpLin = new (typeof(mRes), "Res.Lin.png");
	public static readonly cBitmap BmpPoliLin = new (typeof(mRes), "Res.PoliLin.png");
	public static readonly cBitmap BmpPoli = new (typeof(mRes), "Res.Poli.png");
	public static readonly cBitmap BmpArc = new (typeof(mRes), "Res.Arc.png");
	public static readonly cBitmap BmpBezier = new (typeof(mRes), "Res.Bezier.png");
	public static readonly cBitmap BmpQuadraticBezier = new (typeof(mRes), "Res.QuadraticBezier.png");
	public static readonly cBitmap BmpBorrador = new (typeof(mRes), "Res.Borrador.png");
	public static readonly cBitmap BmpAncho1 = new (typeof(mRes), "Res.Ancho1.png");
	public static readonly cBitmap BmpAncho2 = new (typeof(mRes), "Res.Ancho2.png");
	public static readonly cBitmap BmpAncho3 = new (typeof(mRes), "Res.Ancho3.png");
	public static readonly cBitmap BmpAncho4 = new (typeof(mRes), "Res.Ancho4.png");
	public static readonly cBitmap BmpBorde = new (typeof(mRes), "Res.Borde.png");
	public static readonly cBitmap BmpRelleno = new (typeof(mRes), "Res.Relleno.png");
	public static readonly cBitmap BmpCerrado = new (typeof(mRes), "Res.Cerrado.png");
	public static readonly cBitmap BmpRotarIzq = new (typeof(mRes), "Res.RotarIzq.png");
	public static readonly cBitmap BmpRotarDer = new (typeof(mRes), "Res.RotarDer.png");
	public static readonly cBitmap BmpRecortar = new (typeof(mRes), "Res.Recortar.png");
	public static readonly cBitmap BmpPrint = new (typeof(mRes), "Res.Print.png");
	public static readonly Cursor CurSel = new (typeof(mRes), "Res.Sel.cur");
	public static readonly Cursor CurSelLibre = new (typeof(mRes), "Res.SelLibre.cur");
	public static readonly Cursor CurMano = new (typeof(mRes), "Res.ManoA.cur");
	public static readonly Cursor CurTexto = new (typeof(mRes), "Res.Texto.cur");
	public static readonly Cursor CurMuestra = new (typeof(mRes), "Res.Muestra.cur");
	public static readonly Cursor CurPincel = new (typeof(mRes), "Res.Pincel.cur");
	public static readonly Cursor CurAerosol = new (typeof(mRes), "Res.Aerosol.cur");
	public static readonly Cursor CurFlood = new (typeof(mRes), "Res.Flood.cur");
	public static readonly Cursor CurBlur = new (typeof(mRes), "Res.Blur.cur");
	public static readonly Cursor CurIluminar = new (typeof(mRes), "Res.Iluminar.cur");
	public static readonly Cursor CurRect = new (typeof(mRes), "Res.Rect.cur");
	public static readonly Cursor CurRectRedond = new (typeof(mRes), "Res.RectRedond.cur");
	public static readonly Cursor CurElipse = new (typeof(mRes), "Res.Elipse.cur");
	public static readonly Cursor CurLin = new (typeof(mRes), "Res.Lin.cur");
	public static readonly Cursor CurPoli = new (typeof(mRes), "Res.Poli.cur");
	public static readonly Cursor CurPoliLin = new (typeof(mRes), "Res.PoliLin.cur");
	public static readonly Cursor CurArc = new (typeof(mRes), "Res.Arc.cur");
	public static readonly Cursor CurBezier = new (typeof(mRes), "Res.Bezier.cur");
	public static readonly Cursor CurQuadraticBezier = new (typeof(mRes), "Res.QuadraticBezier.cur");
	public static readonly Cursor CurGlifo = new (typeof(mRes), "Res.Glifo.cur");
}
static class mMod
{	public static readonly string FILTRO_ABRIR = mImaging.GetFileDialogFilter(false);
	public static readonly System.Guid DLG_IMG_GUID = new ("{2ED990AE-0F6A-4181-81A4-D760E4AF21C4}");
	public static readonly System.Guid DLG_EF_GUID = new ("{EBB3D58C-2BB7-42AD-9D81-CE12A5733457}");
		#pragma warning disable CS8618										// Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de declararlo como que admite un valor NULL.
	public static cBitmapBrush FondoTransp; public static cLineStyle GridLin; // Estilos
	public static wMain MainWnd;
	public static kFrames Frames; public static kHerrs Herrs; public static kColores Colores; // Docks
	public static kEfectos Efectos; public static kCfg Cfg;
		#pragma warning restore CS8618
	public static fVista.fPrint? Print;										// Se crea bajo demanda (porque es un poco lento)
}