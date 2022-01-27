using Cursor = System.Windows.Forms.Cursor;
using Wew;
using Wew.Control;
using Wew.Media;

namespace DirectPaint
{
enum eHerr																	// Herramientas con estado
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
	Iluminar,
	Blur
}
abstract class cPrimitivo : Wew.Control.cUndo.cItem							// ** Sólo se descartan los recursos no compartidos: geoms y bmps
{	readonly Point m_ptTamAntImg, m_ptTamImg;
	readonly cBrush m_brBorde, m_brRelleno;
	readonly bool m_bAntialias;
	protected cLineStyle _Trazo; readonly protected float _AnchoLin;
	protected bool _Borde, _Rellenar, _Borrar;
	public readonly fVista Vista;
	public int CantSubDibu;													// Sólo para curvas (ej: pincel, no geom)
	protected cPrimitivo(fVista visPad, int iTamBlob = 0) : base(iTamBlob)	// Sólo para bmp: tam en bytes; en caso de mdl3d se establece al asig mdl
	{	Vista = visPad;
		m_ptTamAntImg = visPad.TamAnt; m_ptTamImg = visPad.Tam;
		m_brBorde = (this is cPrimTxt ? mMod.Colores.BrFuente : mMod.Colores.Borde);
		m_brRelleno = mMod.Colores.Relleno; m_bAntialias = mMod.Cfg.Antialias;
		_Trazo = mMod.Cfg.Trazo; _AnchoLin = mMod.Cfg.AnchoLin;
		_Borde = mMod.Herrs.Borde; _Rellenar = mMod.Herrs.Rellenar; _Borrar = mMod.Herrs.Borrar;
	}
	public Point TamAntImg									{	get	{	return m_ptTamAntImg;}}
	public Point TamImg										{	get	{	return m_ptTamImg;}}
	public abstract void OnMouseDrag(Point ptPt);
	public virtual void OnPresCtrl(Point ptPt)				{}
	public virtual void Pinta(cGraphics g)
	{	cBrush brBorde, brRelleno; bool bLimpiar = false;

		// ** Tomar brushes
		if (!_Borrar)														// ** Pintar
		{	brBorde = m_brBorde; brRelleno = m_brRelleno;
		} else																// ** Borrar
		{	brBorde = Vista.Fondo; brRelleno = brBorde;
			if (!mMod.Grabando)												// En pantalla: limpiar
				bLimpiar = true;
			else															// En bitmap: reemplazar con fondo
				g.PrimitiveBlend = ePrimitiveBlend.Copy;
		}
		// ** Pintar
		g.Antialias = m_bAntialias;
		_Pinta(g, bLimpiar, brBorde, brRelleno);
		g.PrimitiveBlend = ePrimitiveBlend.Default;							// Restablecer
	}
	protected abstract void _Pinta(cGraphics g, bool bLimpiar, cBrush brBorde, cBrush brRelleno);
}
class cPrimPincel : cPrimitivo
{	Point m_ptIni; Array<Point> ma_ptPts;
	static cLineStyle s_lsTrazo = new cLineStyle(eDash.Solid, eCapStyle.Round, eLineJoin.Round);
	public cPrimPincel(fVista visPad, Point ptIni) : base(visPad)
	{	m_ptIni = ptIni; ma_ptPts.MinimumCapacity = 100; _Trazo = s_lsTrazo; // Usar trazo propio
	}
	public override void OnMouseDrag(Point ptPt)			{	ma_ptPts.Add(ptPt); CantSubDibu = ma_ptPts.Count; Vista.AddDibu();}
	protected override void _Pinta(cGraphics g, bool bLimpiar, cBrush brBorde, cBrush brRelleno)
	{	Point ptAnt, pt;

		g.Antialias = false;
			ptAnt = m_ptIni;
			for (int i = 0, iFin = ma_ptPts.Count; i < iFin; i++)
			{	pt = ma_ptPts[i];	if (bLimpiar)	g.DrawLine(ptAnt, pt, mMod.FondoTransp, _AnchoLin, _Trazo);
				g.DrawLine(ptAnt, pt, brBorde, _AnchoLin, _Trazo);
				ptAnt = pt;
			}
		g.Antialias = true;
	}
}
class cPrimLin : cPrimitivo
{	Point m_ptIni, m_ptFin; bool m_bSnap;
	public cPrimLin(fVista visPad, Point ptIni) : base(visPad)	{	m_ptIni = ptIni; m_ptFin = ptIni; m_bSnap = mMod.Cfg.SnapLin;}
	public override void OnMouseDrag(Point ptPt)			{	m_ptFin = ptPt;}
	protected override void _Pinta(cGraphics g, bool bLimpiar, cBrush brBorde, cBrush brRelleno)
	{	if (bLimpiar)	g.DrawLine(m_ptIni, m_ptFin, mMod.FondoTransp, _AnchoLin, _Trazo, m_bSnap);
		g.DrawLine(m_ptIni, m_ptFin, brBorde, _AnchoLin, _Trazo, m_bSnap);
	}
}
class cPrimRect : cPrimitivo
{	bool m_bEsElipse; Rectangle m_rtRect;
	public cPrimRect(fVista visPad, Point ptIni, bool bEsElipse) : base(visPad)	{	m_rtRect.Location = ptIni; m_bEsElipse = bEsElipse;}
	public cPrimRect(fVista visPad, Rectangle rtPlace, bool bEsElipse) : base(visPad) // Para borrar
	{	m_rtRect = rtPlace; m_bEsElipse = bEsElipse; _Borde = false; _Rellenar = true; _Borrar = true;
	}
	public override void OnMouseDrag(Point ptPt)			{	m_rtRect.Size = ptPt - m_rtRect.Location;}
	protected override void _Pinta(cGraphics g, bool bLimpiar, cBrush brBorde, cBrush brRelleno)
	{	// ** Elipse
		if (m_bEsElipse)
		{	if (_Rellenar)
			{	if (bLimpiar)	g.FillEllipse(m_rtRect, mMod.FondoTransp);
			    g.FillEllipse(m_rtRect, brRelleno);
			}
			if (_Borde)
			{	if (bLimpiar)	g.DrawEllipse(m_rtRect, mMod.FondoTransp, _AnchoLin, _Trazo);
			    g.DrawEllipse(m_rtRect, brBorde, _AnchoLin, _Trazo);
			}
		// ** Rectángulo
		} else
		{	if (_Rellenar)
			{	if (bLimpiar)	g.FillRectangle(m_rtRect, mMod.FondoTransp);
			    g.FillRectangle(m_rtRect, brRelleno);
			}
			if (_Borde)
			{	if (bLimpiar)	g.DrawRectangle(m_rtRect, mMod.FondoTransp, _AnchoLin, _Trazo);
			    g.DrawRectangle(m_rtRect, brBorde, _AnchoLin, _Trazo);
			}
		}
	}
}
class cPrimRectRedond : cPrimitivo
{	RoundedRectangle m_rrRect;
	public cPrimRectRedond(fVista visPad, Point ptIni) : base(visPad)	{	m_rrRect.Location = ptIni; m_rrRect.Radius = mMod.Cfg.RrEsquina;}
	public override void OnMouseDrag(Point ptPt)			{	m_rrRect.Size = ptPt - m_rrRect.Location;}
	protected override void _Pinta(cGraphics g, bool bLimpiar, cBrush brBorde, cBrush brRelleno)
	{	if (_Rellenar)
		{    if (bLimpiar)	g.FillRoundedRectangle(m_rrRect, mMod.FondoTransp);
			g.FillRoundedRectangle(m_rrRect, brRelleno);
		}
		if (_Borde)
		{	if (bLimpiar)	g.DrawRoundedRectangle(m_rrRect, mMod.FondoTransp, _AnchoLin, _Trazo);
			g.DrawRoundedRectangle(m_rrRect, brBorde, _AnchoLin, _Trazo);
		}
	}
}
class cPrimBmp : cPrimitivo													// ** El bmp no debe ser compartido porque se descartará con Dispose()
{	cBitmap m_bmpBitmap; Rectangle m_rtRect;
	public cPrimBmp(fVista visPad, cBitmap bmpBitmap) : base(visPad, bmpBitmap.CalculateLineSize() * bmpBitmap.PixelSize.Y) // Dibujar en todo el fondo: es el bitmap del archivo
	{	m_bmpBitmap = bmpBitmap; m_rtRect.Size = TamImg; _Borrar = false;
	}
	public cPrimBmp(fVista visPad, cBitmap bmpBitmap, Rectangle rtRect) : base(visPad, bmpBitmap.CalculateLineSize() * bmpBitmap.PixelSize.Y) // Dibujar sobre la sel
	{	m_bmpBitmap = bmpBitmap; m_rtRect = rtRect;
	}
	protected override void Dispose(bool disposing)			{	m_bmpBitmap.Dispose(); base.Dispose(disposing);}
	public override void OnMouseDrag(Point ptPt)			{}
	protected override void _Pinta(cGraphics g, bool bLimpiar, cBrush brBorde, cBrush brRelleno)
	{	if (bLimpiar)	g.FillRectangle(m_rtRect, mMod.FondoTransp);
		g.DrawBitmap(m_bmpBitmap, m_rtRect, eAlignment.None, eResize.Stretch, 1, eInterpolation.NearestNeighbor);
	}
}
class cPrimTxt : cPrimitivo
{	Rectangle m_rtRect; string m_sTexto; cFont m_fntFnt;
	public cPrimTxt(fVista visPad, Rectangle rtRect, string sTexto) : base(visPad)
	{	m_rtRect = rtRect; m_sTexto = sTexto; m_fntFnt = mMod.Colores.Fuente;
	}
	public override void OnMouseDrag(Point ptPt)			{}
	protected override void _Pinta(cGraphics g, bool bLimpiar, cBrush brBorde, cBrush brRelleno)
	{	if (bLimpiar)	g.DrawText(m_sTexto, m_rtRect, mMod.FondoTransp, m_fntFnt, eTextFormat.Default);
		g.DrawText(m_sTexto, m_rtRect, brBorde, m_fntFnt, eTextFormat.Default);
	}
}
class cPrimCmdLs : cPrimitivo												// Junta todos los dibujos a un cmdls y lo asigna (input) a un ef
{	/*readonly*/ cGpuCommandList m_cmlList; readonly cEffect m_efEf;
	public cPrimCmdLs(fVista visPad, cEffect efEf) : base(visPad)	{	m_efEf = efEf; Compacta(visPad.GetUndo());}
	protected override void Dispose(bool disposing)			{	m_cmlList.Dispose(); base.Dispose(disposing);}
	public cEffect Efecto									{	get	{	return m_efEf;}}
	public override void OnMouseDrag(Point ptPt)			{}
	public override void Pinta(cGraphics g)					{	g.DrawImage(m_efEf, Point.Zero, eInterpolation.NearestNeighbor);}///, eComposite.Xor);}
	public void Compacta(cUndo undUndo, int iPrimIni = -2, int iPrimFin = -2) // Recrea el cmdls a partir del nuevo prim ini o cmdls base
	{	m_cmlList?.Dispose();												// ** Crear ls con todos los dibujos (desde un prim ini o un cmdls)
			m_cmlList = new cGpuCommandList(mMod.MainWnd);
			m_cmlList.Add((cGraphics g) => undUndo.Pinta(g, iPrimIni, iPrimFin)); m_cmlList.EndList();
		if (m_efEf is cSingleInputEffect)									// ** Asig input (ls) a ef
			((cSingleInputEffect)m_efEf).Input = m_cmlList;
		else if (m_efEf is cDualInputEffect)
			((cDualInputEffect)m_efEf).Destination = m_cmlList;
		else if (m_efEf is cCompositeEffect)
			((cCompositeEffect)m_efEf)[0] = m_cmlList;
	}
	protected override void _Pinta(cGraphics g, bool bLimpiar, cBrush brBorde, cBrush brRelleno)	{}
}
abstract class cPrimGeometría : cPrimitivo									// ** La geom no debe ser compartida porque se descartará con Dispose()
{	cGeometry m_geoGeom;
	Point m_ptIni; bool m_bCerrado;
	public cPrimGeometría(fVista visPad, Point ptIni) : base(visPad)
	{	m_ptIni = ptIni; m_bCerrado = mMod.Herrs.Cerrado; m_geoGeom = new cPathGeometry((cPathGeometry.Sink s) => {});
	}
	protected cPrimGeometría(fVista visPad, cGeometry geoBorrar) : base(visPad)	{	m_geoGeom = geoBorrar;} // Para borrar
	protected override void Dispose(bool disposing)
	{	if (m_geoGeom != null)	{	m_geoGeom.Dispose(); m_geoGeom = null;}
		base.Dispose(disposing);
	}
	public sealed override void OnMouseDrag(Point ptPt)
	{	cPathGeometry pg;
	
		pg = new cPathGeometry(												// ** Crear nueva figura
			(cPathGeometry.Sink pgsSink) =>
			{	pgsSink.BeginFigure(m_ptIni, _Rellenar);
					_LlenaFigura(pgsSink, ptPt);
				pgsSink.EndFigure(m_bCerrado);
		    });
		m_geoGeom.Dispose(); m_geoGeom = pg;
	}
	protected override void _Pinta(cGraphics g, bool bLimpiar, cBrush brBorde, cBrush brRelleno)
	{	if (_Rellenar)
		{	if (bLimpiar)	g.FillGeometry(m_geoGeom, mMod.FondoTransp);
		    g.FillGeometry(m_geoGeom, brRelleno);
		}
		if (_Borde)
		{	if (bLimpiar)	g.DrawGeometry(m_geoGeom, mMod.FondoTransp, _AnchoLin, _Trazo);
		    g.DrawGeometry(m_geoGeom, brBorde, _AnchoLin, _Trazo);
		}
	}
	protected abstract void _LlenaFigura(cPathGeometry.Sink pgsSink, Point ptPt);
}
class cGeomArc : cPrimGeometría
{	float m_fAng; Point m_ptRadio;
	public cGeomArc(fVista visPad, Point ptIni) : base(visPad, ptIni)	{	m_fAng = mMod.Cfg.ArcAng; m_ptRadio = mMod.Cfg.ArcRadio;}
	protected override void _LlenaFigura(cPathGeometry.Sink pgsSink, Point ptPt)	{	pgsSink.AddArc(ptPt, m_ptRadio, m_fAng);}
}
class cGeomBezier : cPrimGeometría
{	int m_iCantPts; Point m_pt0, m_pt1, m_ptFin;
	public cGeomBezier(fVista visPad, Point ptIni) : base(visPad, ptIni)	{	m_pt0 = m_pt1 = m_ptFin = ptIni;}
	public override void OnPresCtrl(Point ptPt)								// Agregar pt
	{	if (m_iCantPts == 0)
		{	m_pt0 = m_pt1 = ptPt; m_iCantPts++;
		} else if (m_iCantPts == 1)
		{	m_pt1 = ptPt; m_iCantPts++;
		}
	}
	protected override void _LlenaFigura(cPathGeometry.Sink pgsSink, Point ptPt)
	{	m_ptFin = ptPt; pgsSink.AddBezier(m_pt0, m_pt1, m_ptFin);
	}
}
class cGeomQuadraticBezier : cPrimGeometría
{	Point m_ptCtl, m_ptFin; bool m_bTieneCtl;
	public cGeomQuadraticBezier(fVista visPad, Point ptIni) : base(visPad, ptIni)	{	m_ptCtl = m_ptFin = ptIni;}
	public override void OnPresCtrl(Point ptPt)			{	if (!m_bTieneCtl)	{	m_ptCtl = ptPt; m_bTieneCtl = true;}} // Agregar pt
	protected override void _LlenaFigura(cPathGeometry.Sink pgsSink, Point ptPt)
	{	m_ptFin = ptPt; pgsSink.AddQuadraticBezier(m_ptCtl, m_ptFin);
	}
}
class cGeomPoli : cPrimGeometría
{	Array<Point> ma_ptPts = new Array<Point>(100);
	public cGeomPoli(fVista visPad, Point ptIni) : base(visPad, ptIni)	{}
	public cGeomPoli(fVista visPad, cGeometry geoBorrar) : base(visPad, geoBorrar)	{	_Borde = false; _Rellenar = true; _Borrar = true;} // Para borrar
	protected override void _LlenaFigura(cPathGeometry.Sink pgsSink, Point ptPt)
	{	ma_ptPts.Add(ptPt); pgsSink.AddLines(ma_ptPts.Data, 0, ma_ptPts.Count);
	}
}
class cGeomPoliLin : cPrimGeometría
{	Array<Point> ma_ptPts = new Array<Point>(10);
	public cGeomPoliLin(fVista visPad, Point ptIni) : base(visPad, ptIni)	{}
	public override void OnPresCtrl(Point ptPt)				{	ma_ptPts.Add(ptPt);} // Agregar pt
	protected override void _LlenaFigura(cPathGeometry.Sink pgsSink, Point ptPt)
	{	pgsSink.AddLines(ma_ptPts.Data, 0, ma_ptPts.Count); pgsSink.AddLine(ptPt);
	}
}
class cPrimObj3D : cPrimitivo
{	c3DModel m_mdlMdl; cBitmap m_bmpMdl; Rectangle m_rtRect, m_rtRectFrag;
	readonly cControl m_ctlLienzo; readonly cCamera m_camCam;
	public cPrimObj3D(fVista visPad) : base(visPad)			{	m_ctlLienzo = visPad.GetLienzo(out m_camCam);}
	protected override void Dispose(bool disposing)
	{	if (m_mdlMdl != null)	{	m_mdlMdl.Dispose(); m_mdlMdl = null;}
		if (m_bmpMdl != null)	{	m_bmpMdl.Dispose(); m_bmpMdl = null;}
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
		g3d.SetDefaults(m_camCam); g3d.Render(m_mdlMdl);
	}
	public void TerminaEdición()
	{	if (m_mdlMdl.Visible)
		{	m_camCam.Size = (Point)mMod.MainWnd.BackBuffer.Size;			// Cfg mat view
				m_bmpMdl = mMod.MainWnd.Render3DToBitmap((object sender, c3DGraphics g3d) => // ** Pintar mdl y copiar bmp
					{	g3d.ClearTarget(eColor.Transparent);
						g3d.SetDefaults(m_camCam); g3d.Render(m_mdlMdl);
					});
					//m_bmpMdl.Save("g:\\gg.png");
			m_rtRect.Size = (Point)mMod.MainWnd.BackBuffer.Size;///
				m_rtRectFrag.Size = Vista.Tam; m_rtRectFrag.Size.ScaleTo(m_rtRect.Size); m_rtRectFrag.CenterIn(m_rtRect);
			m_rtRect.Size = Vista.Tam;
		}
		m_mdlMdl.Dispose(); m_mdlMdl = null;								// Liberar mdl
	}
	public override void OnMouseDrag(Point ptPt)			{}
	protected override void _Pinta(cGraphics g, bool bLimpiar, cBrush brBorde, cBrush brRelleno)
	{		if (m_bmpMdl == null)	return;
		g.DrawBitmap(m_bmpMdl, m_rtRect, m_rtRectFrag, eAlignment.None, eResize.Stretch, 1, eInterpolation.NearestNeighbor);
	}
}
class cUndo : Wew.Control.cUndo
{	readonly fVista m_visPad;
	cPrimitivo m_priPrimIni; int m_iBase, m_iPrimIni, m_iPrimActual;		// 1° prim a pintar (en caso de Abrir o compactar).  Ini de undos.  1° (-1 = pintar prim ini) y ult primi de rango a pintar
	int m_iCantDibu, m_iMaxDibus = 1000;									// Cant de dibujos; el tam se considera sólo en el caso de bmps; el tam de los demás dibujos se limita con la cant de dibujos
	static cGpuBitmap s_gbmSurf;
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
		{	for (i = m_iPrimActual - 1; i >= 0 && !(this[i] is cPrimCmdLs); i--)	{}
			m_iPrimIni = i;													// Puede ser -1 (se pintará el prim ini, si existe)
		}
		pri = (cPrimitivo)Undo(out i, out i); m_iPrimActual -= 1;			// ** Retroceder
		m_visPad.Tam = pri.TamAntImg;										// ** Actualizar tam al ant
	}
	public void Redo()
	{	cPrimitivo pri; int i;

		pri = (cPrimitivo)Redo(out i, out i);	if (pri == null)	return;
		m_iPrimActual += 1; if (pri is cPrimCmdLs)	m_iPrimIni = m_iPrimActual; // ** Avanzar; si el prim actual es ls: tomarlo como ini de cola
		m_iCantDibu += 1 + pri.CantSubDibu;									// Contar dibus
		m_visPad.Tam = pri.TamImg;											// ** Actualizar tam al actual
	}
	public void Pinta(cGraphics g, int iPrimIni = -2, int iPrimFin = -2)
	{	if (mMod.Grabando)	g.PrimitiveBlend = ePrimitiveBlend.Copy;
		g.FillRectangle(new Rectangle(Point.Zero, m_visPad.Tam), m_visPad.Fondo); // ** Fondo
		g.PrimitiveBlend = ePrimitiveBlend.Default;
		if (iPrimIni == -2)	iPrimIni = m_iPrimIni;							// Tomar rango pred (el actual)
		if (iPrimFin == -2)	iPrimFin = m_iPrimActual;
		if (iPrimIni == -1)	{	m_priPrimIni?.Pinta(g); iPrimIni = m_iBase;} // ** Pintar todo: pintar pri ini (si hay)
		for (; iPrimIni <= iPrimFin; iPrimIni++)	((cPrimitivo)this[iPrimIni]).Pinta(g); // ** Primis de rango
	}
	public cBitmap Copia(Rectangle rtSel = new Rectangle(), cGeometry geoSelLibre = null, int iPrimIni = -2, int iPrimFin = -2)
	{	Point ptTamSurf, ptTamSurfDipReal;

		if (rtSel.IsEmpty)	rtSel.Size = m_visPad.Tam;						// Sin zona: tomar todo
		ptTamSurfDipReal = rtSel.Size * cGraphics.PixelToDip;
		// ** Redim
		ptTamSurf = (s_gbmSurf?.Size).GetValueOrDefault();
		if ((ptTamSurf.X < rtSel.Width || ptTamSurf.Y < rtSel.Height || s_gbmSurf == null) // La superf es menor que el tam pedido (agrandar)
				|| (m_visPad.Tam.X < ptTamSurf.X / 3 || m_visPad.Tam.Y < ptTamSurf.Y / 3)) // o el tam tot es menor que un tercio de la superf (reducir para ahorrar mem)
		{	
			s_gbmSurf?.Dispose(); s_gbmSurf = new cGpuBitmap(mMod.MainWnd, ptTamSurfDipReal.X, ptTamSurfDipReal.Y);
		}
	try
	{	// ** Pintar
		mMod.Grabando = true;
		mMod.MainWnd.Render2DTo(s_gbmSurf, (cGraphics g) =>
			{	Matrix3x2 mat = Matrix3x2.Identity;
			
				if (rtSel.Location != Point.Zero)	mat.Offset = -rtSel.Location; // ** Ir a la zona sel
				g.SetTransform(mat * Matrix3x2.FromScale(cGraphics.PixelToDip)); // Conv a pix
				Pinta(g, iPrimIni, iPrimFin);								// ** Pintar
				if (geoSelLibre != null)									// ** Despejar sel libre
				{	using (cGeometry geoSelInv = m_visPad.InvSel(geoSelLibre))
					{	g.Antialias = true; g.PrimitiveBlend = ePrimitiveBlend.Copy; // Borrar todo menos la sel
						g.FillGeometry(geoSelInv, eBrush.Transparent);
					}
				}
			});
		// ** Copiar
		return s_gbmSurf.ToBitmap(new Rectangle(Point.Zero, ptTamSurfDipReal));
	} finally
	{	mMod.Grabando = false;
	}
	}
	protected override void OnRemovingItems(int index, int count, bool compacting)
	{	if (compacting)														// ** Compactar
		{	int iIniRango, iFinRango; cPrimitivo pri;

			// ** Crear bmp
			for (iFinRango = index + count - 1, iIniRango = iFinRango; iIniRango >= 0 && !(this[iIniRango] is cPrimCmdLs); iIniRango--)	{} // Buscar cmdls ant (o prim ini) para empezar a pintar nuevo bmp
			pri = new cPrimBmp(m_visPad, Copia(new Rectangle(Point.Zero, ((cPrimitivo)this[iFinRango]).TamImg), null, iIniRango, iFinRango));
			m_priPrimIni?.Dispose(); m_priPrimIni = pri;
			// ** Compactar cmdlss hasta el final
			for (iFinRango++, m_iBase = iFinRango, m_iCantDibu = 0; iFinRango < this.Count; iFinRango++)
			{	pri = (cPrimitivo)this[iFinRango]; m_iCantDibu += 1 + pri.CantSubDibu;	if (!(pri is cPrimCmdLs))	continue; // No es cmdls: omitir
				((cPrimCmdLs)this[iFinRango]).Compacta(this, iIniRango, iFinRango - 1); iIniRango = iFinRango;
			}
			// ** Actualizar rango
			for (m_iPrimIni = m_iPrimActual; m_iPrimIni >= m_iBase && !(this[m_iPrimIni] is cPrimCmdLs); m_iPrimIni--)	{} // Buscar cmdls ant (o prim ini) para empezar a pintar
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
{	public static cCommand EfNuevo = new cCommand("New effect", mRes.BmpNuevoEf);
}
static class mRes
{	public static readonly cBitmap BmpNuevo = new cBitmap(typeof(mRes), "Graf.Nuevo.png");
	public static readonly cBitmap BmpAbrir = new cBitmap(typeof(mRes), "Graf.Abrir.png");
	public static readonly cBitmap BmpGrabar = new cBitmap(typeof(mRes), "Graf.Grabar.png");
	public static readonly cBitmap BmpCortar = new cBitmap(typeof(mRes), "Graf.Cortar.png");
	public static readonly cBitmap BmpCopiar = new cBitmap(typeof(mRes), "Graf.Copiar.png");
	public static readonly cBitmap BmpPegar = new cBitmap(typeof(mRes), "Graf.Pegar.png");
	public static readonly cBitmap BmpDeshacer = new cBitmap(typeof(mRes), "Graf.Deshacer.png");
	public static readonly cBitmap BmpRehacer = new cBitmap(typeof(mRes), "Graf.Rehacer.png");
	public static readonly cBitmap BmpFrames = new cBitmap(typeof(mRes), "Graf.Frames.png");
	public static readonly cBitmap BmpHerr = new cBitmap(typeof(mRes), "Graf.Herr.png");
	public static readonly cBitmap BmpBrush = new cBitmap(typeof(mRes), "Graf.Brush.png");
	public static readonly cBitmap BmpEfecto = new cBitmap(typeof(mRes), "Graf.Efecto.png");
	public static readonly cBitmap BmpCfg = new cBitmap(typeof(mRes), "Graf.Cfg.png");
	public static readonly cBitmap BmpNuevoEf = new cBitmap(typeof(mRes), "Graf.NuevoEf.png");
	public static readonly cBitmap BmpFondoTransp = new cBitmap(typeof(mRes), "Graf.FondoTransp.png");
	public static readonly cBitmap BmpSel = new cBitmap(typeof(mRes), "Graf.Sel.png");
	public static readonly cBitmap BmpSelLibre = new cBitmap(typeof(mRes), "Graf.SelLibre.png");
	public static readonly cBitmap BmpInvSel = new cBitmap(typeof(mRes), "Graf.InvSel.png");
	public static readonly cBitmap BmpMano = new cBitmap(typeof(mRes), "Graf.Mano.png");
	public static readonly cBitmap BmpTexto = new cBitmap(typeof(mRes), "Graf.Texto.png");
	public static readonly cBitmap BmpMuestra = new cBitmap(typeof(mRes), "Graf.Muestra.png");
	public static readonly cBitmap BmpPincel = new cBitmap(typeof(mRes), "Graf.Pincel.png");
	public static readonly cBitmap BmpAerosol = new cBitmap(typeof(mRes), "Graf.Aerosol.png");
	public static readonly cBitmap BmpFlood = new cBitmap(typeof(mRes), "Graf.Flood.png");
	public static readonly cBitmap BmpRect = new cBitmap(typeof(mRes), "Graf.Rect.png");
	public static readonly cBitmap BmpElipse = new cBitmap(typeof(mRes), "Graf.Elipse.png");
	public static readonly cBitmap BmpRectRedond = new cBitmap(typeof(mRes), "Graf.RectRedond.png");
	public static readonly cBitmap BmpLin = new cBitmap(typeof(mRes), "Graf.Lin.png");
	public static readonly cBitmap BmpPoliLin = new cBitmap(typeof(mRes), "Graf.PoliLin.png");
	public static readonly cBitmap BmpPoli = new cBitmap(typeof(mRes), "Graf.Poli.png");
	public static readonly cBitmap BmpArc = new cBitmap(typeof(mRes), "Graf.Arc.png");
	public static readonly cBitmap BmpBezier = new cBitmap(typeof(mRes), "Graf.Bezier.png");
	public static readonly cBitmap BmpQuadraticBezier = new cBitmap(typeof(mRes), "Graf.QuadraticBezier.png");
	public static readonly cBitmap BmpBorrador = new cBitmap(typeof(mRes), "Graf.Borrador.png");
	public static readonly cBitmap BmpAncho1 = new cBitmap(typeof(mRes), "Graf.Ancho1.png");
	public static readonly cBitmap BmpAncho2 = new cBitmap(typeof(mRes), "Graf.Ancho2.png");
	public static readonly cBitmap BmpAncho3 = new cBitmap(typeof(mRes), "Graf.Ancho3.png");
	public static readonly cBitmap BmpAncho4 = new cBitmap(typeof(mRes), "Graf.Ancho4.png");
	public static readonly cBitmap BmpBorde = new cBitmap(typeof(mRes), "Graf.Borde.png");
	public static readonly cBitmap BmpRelleno = new cBitmap(typeof(mRes), "Graf.Relleno.png");
	public static readonly cBitmap BmpCerrado = new cBitmap(typeof(mRes), "Graf.Cerrado.png");
	public static readonly cBitmap BmpRotarIzq = new cBitmap(typeof(mRes), "Graf.RotarIzq.png");
	public static readonly cBitmap BmpRotarDer = new cBitmap(typeof(mRes), "Graf.RotarDer.png");
	public static readonly cBitmap BmpRecortar = new cBitmap(typeof(mRes), "Graf.Recortar.png");
	public static readonly Cursor CurSel = new Cursor(typeof(mRes), "Graf.Sel.cur");
	public static readonly Cursor CurSelLibre = new Cursor(typeof(mRes), "Graf.SelLibre.cur");
	public static readonly Cursor CurMano = new Cursor(typeof(mRes), "Graf.ManoA.cur");
	public static readonly Cursor CurTexto = new Cursor(typeof(mRes), "Graf.Texto.cur");
	public static readonly Cursor CurMuestra = new Cursor(typeof(mRes), "Graf.Muestra.cur");
	public static readonly Cursor CurPincel = new Cursor(typeof(mRes), "Graf.Pincel.cur");
	public static readonly Cursor CurAerosol = new Cursor(typeof(mRes), "Graf.Aerosol.cur");
	public static readonly Cursor CurFlood = new Cursor(typeof(mRes), "Graf.Flood.cur");
	public static readonly Cursor CurBlur = new Cursor(typeof(mRes), "Graf.Blur.cur");
	public static readonly Cursor CurIluminar = new Cursor(typeof(mRes), "Graf.Iluminar.cur");
	public static readonly Cursor CurRect = new Cursor(typeof(mRes), "Graf.Rect.cur");
	public static readonly Cursor CurRectRedond = new Cursor(typeof(mRes), "Graf.RectRedond.cur");
	public static readonly Cursor CurElipse = new Cursor(typeof(mRes), "Graf.Elipse.cur");
	public static readonly Cursor CurLin = new Cursor(typeof(mRes), "Graf.Lin.cur");
	public static readonly Cursor CurPoli = new Cursor(typeof(mRes), "Graf.Poli.cur");
	public static readonly Cursor CurPoliLin = new Cursor(typeof(mRes), "Graf.PoliLin.cur");
	public static readonly Cursor CurArc = new Cursor(typeof(mRes), "Graf.Arc.cur");
	public static readonly Cursor CurBezier = new Cursor(typeof(mRes), "Graf.Bezier.cur");
	public static readonly Cursor CurQuadraticBezier = new Cursor(typeof(mRes), "Graf.QuadraticBezier.cur");
}
static class mMod
{	public const string FILTRO_ABRIR = "Images|*.png;*.jpg;*.gif;*.tif;*.tiff;*.bmp;*.ico;*.dds|All files|*.*";
	public const string FILTRO_GRABAR = "Png|*.png|Jpg|*.jpg|Gif|*.gif|Tiff|*.tif|Bmp|*.bmp|Dds|*.dds";
	public readonly static System.Guid DLG_IMG_GUID = new System.Guid("{2ED990AE-0F6A-4181-81A4-D760E4AF21C4}");
	public readonly static System.Guid DLG_EF_GUID = new System.Guid("{EBB3D58C-2BB7-42AD-9D81-CE12A5733457}");
	public static cBitmapBrush FondoTransp; public static cLineStyle GridLin; public static bool Grabando; // Estilos
	public static wMain MainWnd;
	public static kFrames Frames; public static kHerrs Herrs; public static kColores Colores; // Docks
		public static kEfectos Efectos; public static kCfg Cfg;
}
}