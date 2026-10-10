using Cursor = System.Windows.Forms.Cursor;
using Wew.Control;
using Wew.Media;

namespace DirectPaint;

sealed partial class fVista : cDockControl
{	Point m_ptTamAnt, m_ptTam;
	readonly cCommandState m_csDeshacer, m_csRehacer;
	readonly cUndo m_undUndo;
	readonly cCamera m_camCam;
	int m_iPrintPagIni, m_iPrintPagsAncho;									// ** Impresión
	readonly cLienzo liLienzo; readonly cCtlSel selCtlSel;
	public cBrush Fondo;
	public float Dura;
	public readonly cBitmap.cMetadata? Metadata;
// ** Estat
	static fVista? s_visAnt;
// ** Ctor
public fVista(cBitmap? bmpBmp = null)
{	cScrollableControl sct;

	AllowClose = false;
	m_ptTam = Point.GetMax(Settings.Default.Tam, new Point(1, 1));
	Fondo = eBrush.White;
	m_undUndo = new cUndo(this); m_camCam = new cCamera(1000, 800);
	// ** Agregar ctls
	sct = new cScrollableControl {	Margins = new Rect(0), ClientEndMargin = new Point(10, 10), BackColor = eBrush.LightSteelBlue};
		sct.ClientArea.FocusMode = eFocusMode.ActivateChild;
		AddControl(sct);
	selCtlSel = new cCtlSel(this) {	Visible = false};
	liLienzo = new cLienzo(this, selCtlSel) {	LocationMargin = new Point(10, 10), Width = m_ptTam.X, Height = m_ptTam.Y};
		sct.ClientArea.AddControl(liLienzo);
	// ** Tomar props
	if (bmpBmp is not null)
	{	Fondo = eBrush.Transparent;
		m_ptTam = bmpBmp.Size; Dura = bmpBmp.FrameDuration;
		Metadata = bmpBmp.Metadata; bmpBmp.Metadata = null;					// No perder metas
		m_undUndo.SetPrimIni(new cPrimBmp(this, bmpBmp));
	}
	m_ptTamAnt = m_ptTam;
	mMod.MainWnd.OnVistaRedim(this);
	// ** Agregar cmds
	m_csDeshacer = new cCommandState(eCommand.Undo, DeshacerRehacer_Exec, false);
	m_csRehacer = new cCommandState(eCommand.Redo, DeshacerRehacer_Exec, false);
	r_Commands.AddRange([new cCommandState(eCommand.Cut, Cortar_Exec), new cCommandState(eCommand.Copy, Copiar_Exec)
		, new cCommandState(eCommand.Paste, Pegar_Exec), m_csDeshacer, m_csRehacer]);
}
public override void Close()
{	base.Close();
	mMod.Print?.Cancela(); m_undUndo.Clear(); Metadata?.Dispose();
}
// ** Props
public Point TamAnt									=>	m_ptTamAnt;
public Point Tam															// En dips (= pixeles).  La presentación en pixeles se hace con el zoom
{	get														=>	m_ptTam;
	set
	{	value.Truncate();	if (value == m_ptTam)	return;					// Sin cambio: salir
		m_ptTamAnt = m_ptTam; m_ptTam = value; mMod.MainWnd.OnVistaRedim(this);
	}
}
public bool PuedeMover								{	set	=>	selCtlSel.PuedeMover = value;}
public bool ModoSel									=>	selCtlSel.ModoSel;
public bool ModoSelEf								=>	selCtlSel.ModoSelEf;
public bool ModoTexto								=>	selCtlSel.ModoTexto;
public Rectangle PlaceSel							=>	selCtlSel.PlaceSel;
// ** Mets
public void TerminaEdición()						{	selCtlSel.TerminaEdición();}
public void AplicaEscala(Point ptEscalaTot)			{	liLienzo.Size = m_ptTam * ptEscalaTot; selCtlSel.AplicaEscala();}
public void OnCfgChanged()							{	liLienzo.PrimActual?.OnCfgChanged();}
public void Invalida()								{	liLienzo.Invalidate();}
public void IniConSelEf(Rectangle rtRegión, Wew.dHandler hnOnRedim)	{	selCtlSel.IniConSelEf(rtRegión, hnOnRedim);}
public void InvSel()								{	selCtlSel.InvSel();}
public cPathGeometry InvSel(cGeometry geoSel)		{	return selCtlSel.InvSel(geoSel);}
public void ActualizaTxt()							{	selCtlSel.ActualizaTxt();}
public void SetCursor(Cursor? curCursor)			{	liLienzo.Cursor = curCursor;}
public cSolidBrush? TomaMuestra(Point ptPos, bool bEsPosAbs)	{	return liLienzo.TomaMuestra(ptPos, bEsPosAbs);}
public cControl GetLienzo(out cCamera camCam)		{	camCam = m_camCam; return liLienzo;}
public cUndo GetUndo()								{	return m_undUndo;}
public void AddPrim(Wew.Control.cUndo.cItem item)	{	m_undUndo.Add(item);}
public void AddDibu()								{	m_undUndo.AddDibu();}
public void RedimÍtemUndo(Wew.Control.cUndo.cItem item, int NewSize)	{	m_undUndo.ResizeItem(item, NewSize);}
public void EscribeFme(cBitmap.cFrameEncoder fe)
{	cGpuCommandList cml = new(mMod.MainWnd);

	cml.Add((cGraphics g) => m_undUndo.Pinta(g)); cml.EndList();		// Crear cmdls
	fe.Write(cml, mMod.MainWnd, new RectangleI(default, (PointI)m_ptTam)); // Escribir fme
}
public override bool CanClose(eCloseReason reason = eCloseReason.User)	{	return true;}
public override string ToString()					{	return Text!;} // Para mostrar en ls de frames
protected override void OnEnter()
{	bool bCambioDeVis;

	mMod.MainWnd.OnVistaRedim(this); selCtlSel.OnVistaEnfocada();
	mMod.Herrs.OnVistaEnfocada(this); mMod.Cfg.OnVistaEnfocada(this); mMod.Colores.OnVistaEnfocada(this);
	bCambioDeVis = (s_visAnt is not null && s_visAnt != this); s_visAnt = this;	if (bCambioDeVis)	mMod.Efectos.LimpiaEfectos();
	base.OnEnter();
}
protected override void OnChanged()
{	m_csDeshacer.Enabled = m_undUndo.CanUndo; m_csRehacer.Enabled = m_undUndo.CanRedo; // Actualizar cmds
	Invalida(); mMod.MainWnd.Changed = true;
}
void Cortar_Exec(cCommand command, object? args = null)	{	if (ModoSel)	{	Copiar_Exec(null); selCtlSel.Borra();}}
void Copiar_Exec(cCommand? command, object? args = null)
{		if (ModoTexto)	return;
	if (!ModoSel)	wMain.TerminaEdición();
	using cGeometry? geoSelLibre = (ModoSel ? selCtlSel.GeomSel : null);
	using cBitmap bmp = m_undUndo.Copia((ModoSel ? PlaceSel : default), geoSelLibre);
	
	mClipboard.SetBitmap(bmp);
}
void Pegar_Exec(cCommand command, object? args = null)
{	cBitmap? bmp;

	wMain.TerminaEdición();
	bmp = mClipboard.GetBitmap();	if (bmp is null) return;
	selCtlSel.IniConBmp(new Rectangle((-liLienzo.Location + new Point(10, 10)) / wMain.EscalaTot, bmp.Size), bmp); Changed = true;
}
void DeshacerRehacer_Exec(cCommand command, object? args = null)
{	wMain.TerminaEdición(); mMod.Efectos.LimpiaEfectos();
	if (command == eCommand.Undo)	m_undUndo.Undo();	else	m_undUndo.Redo();
	Changed = true;
}
}
	partial class fVista
	{
// ** cLienzo -----------------------------------------------------------------------------------------------------------------------------
private sealed class cLienzo : cContainer
{// ** Cpos
	readonly fVista m_visPad;
	Point m_ptAnt;
	cPrimitivo? m_priActual;
	readonly cCtlSel selCtlSel;
// ** Ctor
public cLienzo(fVista visPad, cCtlSel selCtlSel)
{	m_visPad = visPad;
	FocusMode = eFocusMode.ActivateChildOrFocusControl;
	this.selCtlSel = selCtlSel;
		AddControl(selCtlSel);
		selCtlSel.IniBorde(this);
}
// ** Props
public cPrimitivo? PrimActual
{	get												=>	m_priActual;
	set												=>	m_priActual = value;
}
// ** Mets
public cSolidBrush? TomaMuestra(Point ptPos, bool bEsPosAbs)
{	System.IntPtr pMem, pMem2;

	if (bEsPosAbs)	{	ptPos = PointToClient(ptPos);	if (!ContentRectangle.Contains(ptPos))	return null;} // Hacer relativo al lienzo; si está afuera: salir
	ptPos += AbsoluteLocation; ptPos *= cGraphics.DipToPixel;				// Hacer relativo al ctl3d; conv a pix
	//¿pintar sin grid
	pMem = pMem2 = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(4);
try
{	mMod.MainWnd.BackBuffer.CopyPixelsTo(pMem, 4, 4, new RectangleI((int)ptPos.X, (int)ptPos.Y, 1, 1));
	return new cSolidBrush(Color.ReadBits(ref pMem2, ePixelFormat.Bits32BGRA));
} finally
{	System.Runtime.InteropServices.Marshal.FreeCoTaskMem(pMem);
}
}
protected override void OnMouseDown(MouseArgs e)
{	Point pt;

		if (e.Button != eMouseButton.Left || selCtlSel.ModoSelEf)	return;	// Btn incorrecto o seleccionando región de ef: salir
	pt = e.Location / wMain.EscalaTot;	if (mMod.Cfg.Redondear)	pt.Truncate(); // Snap
	m_ptAnt = pt;
	wMain.TerminaEdición(); m_priActual = null;
	// ** Crear prim
	switch (mMod.Herrs.Herr)
	{	case eHerr.Sel:				selCtlSel.IniConSel(pt, false); break;
		case eHerr.SelLibre:		selCtlSel.IniConSel(pt, true); break;
		case eHerr.Texto:			selCtlSel.IniConTxt(pt); break;
		case eHerr.Muestra:
			cSolidBrush? sbr;

			sbr = TomaMuestra(e.Location, false);	if (sbr is null)	break; // Afuera: saltar
			if (mMod.Herrs.Borde)		mMod.Colores.Borde = sbr;		// Tomar colores
			if (mMod.Herrs.Rellenar)	mMod.Colores.Relleno = sbr;
			mMod.Colores.BrFuente = sbr;
			mMod.Herrs.Herr = mMod.Herrs.HerrPrev;						// Regresar a la herr prev
			break;
		case eHerr.Pincel:			m_priActual = new cPrimPincel(m_visPad, pt); break;
		case eHerr.Aerosol:			m_priActual = new cPrimPincel(m_visPad, pt); break;//¿
		case eHerr.Flood:			break;//¿
		case eHerr.Blur:			break;//
		case eHerr.Iluminar:		break;//
		case eHerr.Rect:			m_priActual = new cPrimRect(m_visPad, pt); break;
		case eHerr.RectRedond:		m_priActual = new cPrimRectRedond(m_visPad, pt); break;
		case eHerr.Elipse:			m_priActual = new cPrimElip(m_visPad, pt); break;
		case eHerr.Lin:				m_priActual = new cPrimLin(m_visPad, pt); break;
		case eHerr.Poli:			m_priActual = new cGeomPolígono(m_visPad, pt); break;
		case eHerr.PoliLin:			m_priActual = new cGeomPoliLin(m_visPad, pt); break;
		case eHerr.Arc:				m_priActual = new cGeomArc(m_visPad, pt); break;
		case eHerr.Bezier:			m_priActual = new cGeomBezier(m_visPad, pt); break;
		case eHerr.QuadraticBezier:	m_priActual = new cGeomQuadraticBezier(m_visPad, pt); break;
		case eHerr.Glifo:			m_priActual = new cGeomGlifo(m_visPad, pt, kHerrs.Fuente, kHerrs.Glifo); break;
	}
	// ** Empezar edición
	if (m_priActual is not null)	mMod.Efectos.LimpiaEfectos();
}
protected override void OnMouseMove(MouseArgs e)
{	Point pt;

	pt = e.Location / wMain.EscalaTot;	if (mMod.Cfg.Redondear)	pt.Truncate(); // Snap
	wMain.MuestraStatPos($"({(int)pt.X}, {(int)pt.Y})");
		if (e.Button != eMouseButton.Left || pt == m_ptAnt) return;		// Sin arrast: salir
	m_ptAnt = pt;
	if (m_priActual is not null)	{	m_priActual.OnMouseDrag(pt); m_visPad.Changed = true;}
	selCtlSel.OnMouseDrag(pt);
}
protected override void OnLostCapture()				{	m_priActual?.OnMouseUp(); selCtlSel.OnMouseUp();}
protected override void OnKeyDownPreview(ref KeyArgs e)
{	switch (e.Key)
	{	case eKey.OemPlus:	mMod.MainWnd.Zoom(m_visPad, true); e.Handled = true;
			break;
		case eKey.OemMinus:	mMod.MainWnd.Zoom(m_visPad, false); e.Handled = true;
			break;
		default:
			if (m_priActual is not null && e.Control)					// ** Agregar pt
			{	m_priActual.OnPresCtrl(m_ptAnt); m_visPad.Changed = true; e.Handled = true;
			}
			break;
	}
}
protected override void OnPaint(PaintArgs e)
{	e.Graphics.Transform = wMain.EscalaMatriz;							// Aplicar escala
	// ** Fondo
	e.Graphics.FillRectangle(new Rectangle(default, m_visPad.m_ptTam), mMod.FondoTransp);
	// ** Contenido
	e.Graphics.PushLayer();												// Usar layer con primitive blend
try
{		m_visPad.m_undUndo.Pinta(e.Graphics);
		PrimActual?.Pinta(e.Graphics);
} finally
{	e.Graphics.PopLayer();
}
	e.Graphics.Transform = wMain.EscalaMatriz;							// Reaplicar escala
	// ** 3D
	mMod.Efectos.Pinta3D(e);
	// ** Grid
	if (mMod.Cfg.Grid && wMain.EscalaTot.X >= 8)
	{	float fAncho, fAlto, f;

	//¿sólo la parte visible
		fAncho = m_visPad.m_ptTam.X; fAlto = m_visPad.m_ptTam.Y; e.Graphics.Antialias = false;
		for (f = -0.5f; f < fAncho; f++)	e.Graphics.DrawVerticalLine(f, 0, fAlto, eBrush.Gray, 1, mMod.GridLin);
		for (f = -0.5f; f < fAlto; f++)		e.Graphics.DrawHorizontalLine(0, fAncho, f, eBrush.Gray, 1, mMod.GridLin);
	}
}
protected override void OnSizeChanged()				{	m_visPad.m_camCam.Size = Size;}
}
// ** cCtlSel -----------------------------------------------------------------------------------------------------------------------------
private sealed class cCtlSel : cContainer
{// ** Tps
	enum eModo
	{	Ninguno	= 0,
		Sel,
		SelEf,
		Prim,
		Bitmap,
		Texto
	}
// ** Cpos
	readonly fVista m_visPad;
	eModo m_modModo;
	Point m_ptIni; Rectangle m_rtPlaceSel;
	cGeometry? m_geoGeom; Wew.Array<Point> ma_ptGeom;					// ** Sel libre
	bool m_bBlqGeom; cTransformedGeometry? m_tgEscalada;				// La geo escalada es para dibujar el focusrect sin transform
	Wew.dHandler? m_hnOnRedim;
	Rectangle m_rtPrim;
	cBitmap? m_bmpBitmap;
	readonly cTextBox txtTexto;
	readonly cResizableBorder rzbBorde;
// ** Ctor/dtor
	public cCtlSel(fVista visPad)
	{	m_visPad = visPad;
		ma_ptGeom.MinimumCapacity = 100;
		txtTexto = new cTextBox {	BackColor = null, BorderStyle = eBorderStyle.None, Margins = new Rect(0), Visible = false};
			AddControl(txtTexto);
		rzbBorde = new cResizableBorder {	Control = this, Visible = false};
			rzbBorde.KeyDown += rzbBorde_KeyDown;
		HitTestTransparent = eHitTestTransparent.Background;
	}
	public void IniBorde(cLienzo liLienzo)			{	liLienzo.AddControl(rzbBorde);}
// ** Props
	public Rectangle PlaceSel						=>	m_rtPlaceSel;	// Sel sin escala y del tam total
	public bool PuedeMover
	{	set
		{	rzbBorde.HitTestTransparent = (value ? eHitTestTransparent.None : eHitTestTransparent.ClientArea);
			rzbBorde.Cursor = (value ? mRes.CurMano : null);
		}
	}
	public bool ModoSel								=>	m_modModo == eModo.Sel;
	public bool ModoSelEf							=>	m_modModo == eModo.SelEf;
	public bool ModoTexto							=>	m_modModo == eModo.Texto;
	public cGeometry? GeomSel
	{	get
		{		if (m_geoGeom is null)	return null;
			return new cTransformedGeometry(m_geoGeom, Matrix3x2.FromTranslation(m_rtPlaceSel.Location)); // Conv la geo a coords de lienzo
		}
		private set
		{	m_geoGeom?.Dispose(); m_geoGeom = value;
			m_tgEscalada?.Dispose(); m_tgEscalada = null;
		}
	}
	cPrimitivo? m_Primitivo
	{	get											=>	m_visPad.liLienzo.PrimActual;
		set											=>	m_visPad.liLienzo.PrimActual = value;
	}
// ** Mets
	public void IniConSel(Point ptPos, bool bLibre)
	{	m_modModo = eModo.Sel; m_ptIni = ptPos;
		if (!bLibre)														// ** Usar rect
			Bounds = new Rectangle(ptPos, default) * wMain.EscalaTot;
		else																// ** Usar poli
		{	Bounds = new Rectangle(default, m_visPad.m_ptTam) * wMain.EscalaTot; // Inicialmente, tomar todo el lienzo
			m_geoGeom = new cPathGeometry((cPathGeometry.Sink s) => {}); ma_ptGeom.Clear(); // ** Crear geom
		}
		Show();
	}
	public void IniConSelEf(Rectangle rtRegión, Wew.dHandler hnOnRedim)
	{	if (m_modModo != eModo.SelEf)										// No perder sel
		{	TerminaEdición();
			m_modModo = eModo.SelEf;
			Bounds = rtRegión * wMain.EscalaTot;
			Show();
			PuedeMover = true; rzbBorde.Show(); rzbBorde.Focus();
		}
		m_hnOnRedim = hnOnRedim; hnOnRedim();								// Aplicar región actual desde ahora
	}
	public void IniConBmp(Rectangle rtPlace, cBitmap bmpBmp)
	{	m_modModo = eModo.Bitmap; m_bmpBitmap = bmpBmp;
		Bounds = rtPlace * wMain.EscalaTot;
		Show();
		PuedeMover = true; rzbBorde.Show(); rzbBorde.Focus();
	}
	public void IniConTxt(Point ptPos)
	{	m_modModo = eModo.Texto;
		Bounds = new Rectangle(ptPos, new Point(100, 20)) * wMain.EscalaTot;
		Show();
		txtTexto.Text = null; txtTexto.Show(); ActualizaTxt(); txtTexto.Focus();
		PuedeMover = false; rzbBorde.Show();
	}
	public void OnMouseDrag(Point ptPos)
	{	// ** Sel libre
		if (m_modModo == eModo.Sel && m_geoGeom is not null)
		{	GeomSel = new cPathGeometry((cPathGeometry.Sink pgsSink) =>		// Crear nuevo poli
				{	pgsSink.BeginFigure(m_ptIni, true);
						ma_ptGeom.Add(ptPos); pgsSink.AddLines(ma_ptGeom.Data, 0, ma_ptGeom.Count);
					pgsSink.EndFigure(true);
				});
			Invalidate();
		// ** Demás modos: actualizar tam
		} else
			Bounds = (Rectangle)new Rect(m_ptIni, ptPos) * wMain.EscalaTot;
	}
	public void OnMouseUp()
	{		if (Size.GetMinComponent().X <= 1)	{	m_Cancela(); return;}	// Sel muy pequeña: cancelar, salir
		if (m_modModo == eModo.Sel)
		{	if (m_geoGeom is not null)										// ** Libre
			{	Rectangle rt;

				rt = m_geoGeom.GetBounds();	if (!rt.IsValid())	{	m_Cancela(); return;} // Sel vacía: cancelar sel, salir
				m_bBlqGeom = true; Bounds = rt * wMain.EscalaTot; m_bBlqGeom = false; // Rodear la sel
				if (rt.Location != default)									// Conv la geo a coords de ctlsel
					GeomSel = new cTransformedGeometry(m_geoGeom, Matrix3x2.FromTranslation(-rt.Location));
			}
			PuedeMover = false;
			rzbBorde.Show(); rzbBorde.Focus();
		} else if (m_Primitivo is not null)
		{	m_modModo = eModo.Prim; m_rtPrim = m_Primitivo.Lims;	if (!m_rtPrim.IsValid())	{	m_Cancela(); return;}	// Sel muy pequeña: cancelar, salir
			Bounds = m_rtPrim * wMain.EscalaTot;
			Show();
			PuedeMover = true;
			rzbBorde.Show(); rzbBorde.Focus();
		}
	}
	public void AplicaEscala()
	{	m_bBlqGeom = true; Bounds = m_rtPlaceSel * wMain.EscalaTot; m_bBlqGeom = false; // ** Redim
		if (m_tgEscalada is not null)	{	m_tgEscalada.Dispose(); m_tgEscalada = null;}
		ActualizaTxt();
	}
	public void InvSel()
	{		if (m_modModo != eModo.Sel)	return;
		using cGeometry geoSel = (m_geoGeom is not null ? GeomSel! : new cRectangleGeometry(m_rtPlaceSel)); // ** Si no hay sel libre: tomar la sel rect

		GeomSel = InvSel(geoSel);											// Inv sel
		OnMouseUp();														// Conv la geo a coords de ctlsel
		Invalidate();
	}
	public cPathGeometry InvSel(cGeometry geoSel)
	{	cPathGeometry pgRes; using cRectangleGeometry rgTodo = new (new Rectangle(default, m_visPad.m_ptTam));

		pgRes = new cPathGeometry((cPathGeometry.Sink pgsSink) =>
			pgsSink.AddCombinedGeometries(rgTodo, geoSel, cPathGeometry.eCombine.Exclude));
		return pgRes;
	}
	public void ActualizaTxt()
	{		if (m_modModo != eModo.Texto)	return;
		txtTexto.Font?.Dispose(); txtTexto.Font = new cFont(mMod.Colores.Fuente.Name, mMod.Colores.Fuente.Size * wMain.EscalaTot.X); // Aplicar fnt y escala
		txtTexto.TextColor = mMod.Colores.BrFuente;
	}
	public void TerminaEdición()
	{	switch (m_modModo)
		{	case eModo.Bitmap:	m_visPad.m_undUndo.Add(new cPrimBmp(m_visPad, m_bmpBitmap!, m_rtPlaceSel)); m_bmpBitmap = null;
				m_visPad.Changed = true;
				break;
			case eModo.Texto:	m_visPad.m_undUndo.Add(new cPrimTxt(m_visPad, m_rtPlaceSel, txtTexto.Text));
				m_visPad.Changed = true;
				break;
			case eModo.Prim:	m_visPad.m_undUndo.Add(m_Primitivo!); m_Primitivo = null;
				m_visPad.Changed = true;
				break;
		}
		m_Cancela();
	}
	public void Borra()
	{	switch (m_modModo)
		{	case eModo.Sel:													// ** Hay sel: borrar sel
				if (m_geoGeom is null)										// Rect
					m_visPad.m_undUndo.Add(new cPrimRect(m_visPad, m_rtPlaceSel));
				else														// Libre
				{	m_visPad.m_undUndo.Add(new cGeomPolígono(m_visPad, GeomSel!)); m_geoGeom = null; // No hacer dispose porque la geom se compartió
				}
				m_visPad.Changed = true;
				break;
		}
		m_Cancela();
	}
	public void OnVistaEnfocada()					{	OnVisibleChanged();}
	protected override void OnVisibleChanged()
	{	if (Visible)														// Mostrar coords
		{	RectangleI rti;
		
			rti = (RectangleI)m_rtPlaceSel;
			wMain.MuestraStatRect($"({rti.X}, {rti.Y}) - ({rti.Right}, {rti.Bottom})");
			wMain.MuestraStatTam($"{rti.Width} x {rti.Height}");
		} else
		{	wMain.MuestraStatRect(null); wMain.MuestraStatTam(null);
		}	
	}
	protected override void OnBoundsChanged()
	{	Rectangle rtSelNueva;

		// ** Redim sel
		rtSelNueva = RealBounds / wMain.EscalaTot;
		if (m_geoGeom is not null && !m_bBlqGeom)							// El usr está ajustando el tam: redim geo
		{	GeomSel = new cTransformedGeometry(m_geoGeom, Matrix3x2.FromScale(rtSelNueva.Size / m_rtPlaceSel.Size));
		}
		// ** Redim lims
		m_rtPlaceSel = rtSelNueva;
		rzbBorde.UpdateBounds();
		// ** Redim prim
		if (m_modModo == eModo.Prim)
		{	if (Visible)
				m_Primitivo!.Transform = Matrix3x2.FromTranslation(-m_rtPrim.Location) // Llevar prim a orig de lienzo
					* new Matrix3x2(0, (rtSelNueva.Size / m_rtPrim.Size), rtSelNueva.Location); // Redim y mover prim
		// ** Selec región de ef
		} else if (m_modModo == eModo.SelEf)
		{	m_hnOnRedim?.Invoke();
		}
		// ** Mostrar coords
		OnVisibleChanged();
	}
	protected override void OnPaint(PaintArgs e)
	{	Rectangle rt = ClientRectangle;

		// ** Bmp
		if (m_bmpBitmap is not null)
			e.Graphics.DrawBitmap(m_bmpBitmap, rt, eAlignment.None, eResize.Stretch, 1, eInterpolation.NearestNeighbor);
		// ** Sel libre: pintar polig
		else if (m_geoGeom is not null)
		{	m_tgEscalada ??= new cTransformedGeometry(m_geoGeom, wMain.EscalaMatriz); // ** Crear geom escalada
			e.Graphics.Antialias = false; e.Graphics.DrawGeometry(m_tgEscalada, eBrush.SystemFocusRectangle);
		// ** Sel rect sin brd: pintar focus rect
		} else if (!rzbBorde.Visible && m_Primitivo is null)
			e.Graphics.DrawFocusRectangle(rt);
	}
	protected override void OnKeyDownPreview(ref KeyArgs e)
	{	if (e.Key == eKey.Escape)	rzbBorde_KeyDown(null, ref e);			// Escape en texto
	}
	void rzbBorde_KeyDown(object? sender, ref KeyArgs e)
	{	switch (e.Key)
		{	case eKey.Escape:												// ** Cancelar
				m_Cancela(); e.Handled = true;
				break;
			case eKey.Delete:												// ** Eliminar o cancelar
				Borra(); e.Handled = true;
				break;
			case eKey.Enter:												// ** Aplicar
				if (m_modModo is eModo.Bitmap or eModo.Prim)	{	TerminaEdición(); e.Handled = true;}
				break;
		}
	}
	void m_Cancela()
	{	m_bmpBitmap?.Dispose(); m_bmpBitmap = null;
		m_Primitivo?.Dispose(); m_Primitivo = null;
		txtTexto.Hide();
		GeomSel = null;
		m_hnOnRedim = null;
		if (m_modModo == eModo.SelEf)	mMod.Efectos.LimpiaEfectos();
		m_modModo = eModo.Ninguno; rzbBorde.Hide();
			if (!Visible)	return;											// Ya cancelado: evitar llamada recur al cambiar de herr; salir
		Hide(); Bounds = default;	if (mMod.Herrs.Herr == eHerr.Mano)	mMod.Herrs.Herr = mMod.Herrs.HerrPrev;
		m_visPad.Changed = true;
	}
}
// ** fPrint ------------------------------------------------------------------------------------------------------------------------------
public sealed class fPrint : cDockControl
{	Rectangle m_rtOrig;
	readonly cPrintPreview pvwPrint;
	private static fVista s_visBusca(int iPag)
	{	int iIdx;

		iIdx = mMod.Frames.Frames.BinarySearch(null, new Wew.cComparer<fVista?>((fVista? a, fVista? b) => // Buscar null para no crear vista temp
			{	int iA = (a is null ? iPag : a.m_iPrintPagIni), iB = (b is null ? iPag : b.m_iPrintPagIni);
			
				return iA - iB;
			}));
		if (iIdx < 0)	iIdx = ~iIdx - 1;									// No hallado exacta: tomar el ant
		return mMod.Frames.Frames[iIdx];
	}
	public fPrint()
	{	Text = "Print"; AllowClose = false;
		pvwPrint = new cPrintPreview {	Margins = new Rect(5)};
			pvwPrint.SettingsChanged += pvwPrint_SettingsChanged;
			pvwPrint.BeginPrint += pvwPrint_BeginPrint;
			pvwPrint.PreparePage += pvwPrint_PreparePage;
			pvwPrint.PaintPage += pvwPrint_PaintPage;
			pvwPrint.Cancelled += pvwPrint_Cancelled;
			AddControl(pvwPrint);
	}
	public void Muestra()
	{	m_Limpia();															// Limpiar
		Show();
		pvwPrint_SettingsChanged(null);										// Cfg cuando el prn ya está insertado en wnd
	}
	public void Cancela()									{	pvwPrint_Cancelled(null);}
	private void pvwPrint_SettingsChanged(object? sender)					// Calc cant pags
	{	int iPags = 0; Point ptTamImg, ptPrintArea = pvwPrint.GetPrintArea().Size; int iFils;

		foreach (fVista vis in mMod.Frames.Frames)
		{	ptTamImg = (vis.m_ptTam / ptPrintArea).GetCeiling();
			iFils = (int)ptTamImg.Y; vis.m_iPrintPagIni = iPags; vis.m_iPrintPagsAncho = (int)ptTamImg.X;
			iPags += iFils * vis.m_iPrintPagsAncho;
		}
		pvwPrint.PageCount = iPags;
	}
	private void pvwPrint_BeginPrint(object sender, ref bool cancel)	{	cUndo.RedimSurf(pvwPrint.GetPrintArea().Size);} // ** Se usará sólo el tam necesario de surf y el mismo para todos los fragmentos
	private void pvwPrint_PreparePage(object sender, cGraphics g, bool IsPrinting, int page, Rectangle destination)
	{	fVista vis; Point ptColFil;

		vis = s_visBusca(page);	if (vis.m_iPrintPagsAncho == 0)	return;
		page -= vis.m_iPrintPagIni; ptColFil.Y = page / vis.m_iPrintPagsAncho; ptColFil.X = page % vis.m_iPrintPagsAncho; // ** Calc orig
		m_rtOrig = new Rectangle(destination.Size * ptColFil, destination.Size);
		if (IsPrinting)														// ** Print: componer img
		{	mMod.MainWnd.Render2DTo(cUndo.Surf!, (cGraphics g) => vis.m_undUndo.PintaSinEscala(g, -m_rtOrig.Location));
		}
		m_rtOrig.Intersect(new Rectangle(default, vis.m_ptTam));			// Calc tam exacto de fragmento
	}
	private void pvwPrint_PaintPage(object sender, cGraphics g, bool IsPrinting, int page, Rectangle destination)
	{	destination.Size = m_rtOrig.Size;									// Recortar dest
		if (!IsPrinting)													// ** Vista
		{	g.Clip = destination;											// Aplicar dest
			g.Transform = Matrix3x2.FromTranslation(destination.Location - m_rtOrig.Location); // Ir al orig
			s_visBusca(page).m_undUndo.Pinta(g);
		} else																// ** Print
			g.DrawBitmap(cUndo.Surf!, destination);
	}
	private void pvwPrint_Cancelled(object? sender)			{	m_Limpia(); Hide();}
	private void m_Limpia()									{	pvwPrint.PageCount = 0;}
}
	}