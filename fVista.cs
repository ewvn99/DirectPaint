using Cursor = System.Windows.Forms.Cursor;
using CultInf = System.Globalization.CultureInfo;
using Wew.Control;
using Wew.Media;

namespace DirectPaint
{
partial class fVista : cDockControl
{// ** Tps
	public delegate void dRedimEf();
// ** Cpos
	Point m_ptTamAnt, m_ptTam;
	readonly cCommandState m_csDeshacer, m_csRehacer;
	readonly cUndo m_undUndo;
	readonly cCamera m_camCam;
	readonly cLienzo liLienzo; readonly cCtlSel selCtlSel;
	public cBrush Fondo;
	public float Dura;
	public readonly cBitmap.cMetadata Metadata;
// ** Estat
	static fVista s_visAnt;
// ** Ctor/dtor
public fVista(cBitmap bmpBmp = null)
{	cScrollableControl sct;

	AllowClose = false;
	m_ptTam = Settings.Default.Tam; Fondo = eBrush.Transparent;
	m_undUndo = new cUndo(this); m_camCam = new cCamera(1000, 800);
	// ** Agregar ctls
	sct = new cScrollableControl() {	Margins = new Rect(0), BackColor = eBrush.LightSteelBlue};
		sct.ClientArea.FocusMode = eFocusMode.ActivateChild;
		AddControl(sct);
	selCtlSel = new cCtlSel(this) {	Visible = false};
	liLienzo = new cLienzo(this, selCtlSel) {	LocationMargin = new Point(10, 10), Width = m_ptTam.X, Height = m_ptTam.Y};
		liLienzo.AddControl(selCtlSel);
		sct.ClientArea.AddControl(liLienzo);
	selCtlSel.IniBorde(liLienzo);
	// ** Tomar props
	if (bmpBmp != null)
	{	m_ptTam = bmpBmp.Size; Dura = bmpBmp.FrameDuration;
		Metadata = bmpBmp.Metadata; bmpBmp.Metadata = null;					// No perder metas
		m_undUndo.SetPrimIni(new cPrimBmp(this, bmpBmp));
	}
	m_ptTamAnt = m_ptTam;
	mMod.MainWnd.OnVistaRedim(this);
	// ** Agregar cmds
	m_csDeshacer = new cCommandState(eCommand.Undo, DeshacerRehacer_Exec, false);
	m_csRehacer = new cCommandState(eCommand.Redo, DeshacerRehacer_Exec, false);
	r_Commands.AddRange(new cCommandState(eCommand.Cut, Cortar_Exec), new cCommandState(eCommand.Copy, Copiar_Exec)
		, new cCommandState(eCommand.Paste, Pegar_Exec)
		, m_csDeshacer, m_csRehacer);
}
public override void Close()
{	base.Close();
	m_undUndo.Clear(); Metadata?.Dispose();
}
// ** Props
public Point TamAnt											{	get	{	return m_ptTamAnt;}}
public Point Tam															// En dips (= pixeles).  La presentación en pixeles se hace con el zoom
{	get														{	return m_ptTam;}
	set
	{	value.Truncate();	if (value == m_ptTam)	return;					// Sin cambio: salir
		m_ptTamAnt = m_ptTam; m_ptTam = value; mMod.MainWnd.OnVistaRedim(this);
	}
}
public bool PuedeMover										{	set	{	selCtlSel.PuedeMover = value;}}
public bool ModoSel											{	get	{	return selCtlSel.ModoSel;}}
public bool ModoSelEf										{	get	{	return selCtlSel.ModoSelEf;}}
public bool ModoTexto										{	get	{	return selCtlSel.ModoTexto;}}
public Rectangle PlaceSel									{	get	{	return selCtlSel.PlaceSel;}}
public Rectangle PlaceSelReal								{	get	{	return selCtlSel.PlaceSelReal;}}
// ** Mets
public void TerminaEdición()								{	selCtlSel.TerminaEdición();}
public void AplicaEscala(Point ptEscalaTot)					{	liLienzo.Size = m_ptTam * ptEscalaTot; selCtlSel.AplicaEscala();}
public void Invalida()										{	liLienzo.Invalidate();}
public void IniConSelEf(Rectangle rtRegión, dRedimEf reOnRedim)	{	selCtlSel.IniConSelEf(rtRegión, reOnRedim);}
public void InvSel()										{	selCtlSel.InvSel();}
public cPathGeometry InvSel(cGeometry geoSel)				{	return selCtlSel.InvSel(geoSel);}
public void ActualizaTxt()									{	selCtlSel.ActualizaTxt();}
public void SetCursor(Cursor curCursor)						{	liLienzo.Cursor = curCursor;}
public cSolidBrush TomaMuestra(Point ptPos, bool bEsPosAbs)	{	return liLienzo.TomaMuestra(ptPos, bEsPosAbs);}
public cControl GetLienzo(out cCamera camCam)				{	camCam = m_camCam; return liLienzo;}
public cUndo GetUndo()										{	return m_undUndo;}
public void AddPrim(Wew.Control.cUndo.cItem item)			{	m_undUndo.Add(item);}
public void AddDibu()										{	m_undUndo.AddDibu();}
public void RedimÍtemUndo(Wew.Control.cUndo.cItem item, int NewSize)	{	m_undUndo.ResizeItem(item, NewSize);}
public cBitmap CompónImg(Rectangle rtSel = new Rectangle(), cGeometry geoSelLibre = null, int iPrimIni = -2, int iPrimFin = -2)
{	return m_undUndo.Copia(rtSel, geoSelLibre, iPrimIni, iPrimFin);
}
public override bool CanClose(eCloseReason reason = eCloseReason.User)	{	return true;}
public override string ToString()							{	return Text;}
protected override void OnEnter()
{	bool bCambioDeVis;

	mMod.MainWnd.OnVistaRedim(this);
	mMod.Herrs.OnVistaEnfocada(this); mMod.Cfg.OnVistaEnfocada(this); mMod.Colores.OnVistaEnfocada(this);
	bCambioDeVis = (s_visAnt != null && s_visAnt != this); s_visAnt = this;	if (bCambioDeVis)	mMod.Efectos.LimpiaEfectos();
	base.OnEnter();
}
protected override void OnChanged()
{	m_csDeshacer.Enabled = m_undUndo.CanUndo; m_csRehacer.Enabled = m_undUndo.CanRedo; // Actualizar cmds
	Invalida(); mMod.MainWnd.Changed = true;
}
private void Cortar_Exec(cCommandState command, object args = null)	{	if (ModoSel)	{	Copiar_Exec(null); selCtlSel.Borra();}}
private void Copiar_Exec(cCommandState command, object args = null)
{		if (ModoTexto)	return;
	if (!ModoSel)	wMain.TerminaEdición();
	using (cGeometry geoSelLibre = (ModoSel ? selCtlSel.GeomSel : null))
		using (cBitmap bmp = CompónImg((ModoSel ? PlaceSel : default(Rectangle)), geoSelLibre))
			mClipboard.SetImage(bmp);
}
private void Pegar_Exec(cCommandState command, object args = null)
{	cBitmap bmp;

	wMain.TerminaEdición();
	bmp = mClipboard.GetImage();	if (bmp == null) return;
	selCtlSel.IniConBmp(new Rectangle(Point.Zero, bmp.Size), bmp); Changed = true;
}
private void DeshacerRehacer_Exec(cCommandState command, object args = null)
{	wMain.TerminaEdición(); mMod.Efectos.LimpiaEfectos();
	if (command == m_csDeshacer)	m_undUndo.Undo();	else	m_undUndo.Redo();
	Changed = true;
}
}
	partial class fVista : cDockControl
	{
// ** cLienzo -----------------------------------------------------------------------------------------------------------------------------
private class cLienzo : cContainer
{// ** Cpos
	fVista m_visPad;
	Point m_ptAnt;
	cPrimitivo m_priActual;
	readonly cCtlSel selCtlSel;
// ** Ctor/dtor
public cLienzo(fVista visPad, cCtlSel selCtlSel)
{	m_visPad = visPad; this.selCtlSel = selCtlSel; FocusMode = eFocusMode.FocusControl;
}
// ** Mets
public cSolidBrush TomaMuestra(Point ptPos, bool bEsPosAbs)
{	System.IntPtr pMem, pMem2;

	if (bEsPosAbs)	{	ptPos = PointToClient(ptPos);	if (!ContentRectangle.Contains(ptPos))	return null;} // Hacer relativo al lienzo; si está afuera: salir
	ptPos += AbsoluteLocation; ptPos *= cGraphics.DipToPixel;				// Hacer relativo al ctl3d; conv a pix
	///pintar sin grid
	pMem = pMem2 = Wew.mHelper.Malloc(4);
try
{	mMod.MainWnd.CopyPixelsTo(pMem, 4, new RectangleI((int)ptPos.X, (int)ptPos.Y, 1, 1));
	return new cSolidBrush(Color.ReadBits(ref pMem2, ePixelFormat._32bppBGRA));
} finally
{	Wew.mHelper.Free(pMem);
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
	{	case eHerr.Sel:			selCtlSel.IniConSel(pt, false); break;
		case eHerr.SelLibre:	selCtlSel.IniConSel(pt, true); break;
		case eHerr.Texto:		selCtlSel.IniConTxt(pt); break;
		case eHerr.Muestra:
			cSolidBrush sbr;

			sbr = TomaMuestra(e.Location, false);	if (sbr == null)	break; // Afuera: saltar
			if (mMod.Herrs.Borde)		mMod.Colores.Borde = sbr;			// Tomar colores
			if (mMod.Herrs.Rellenar)	mMod.Colores.Relleno = sbr;
			mMod.Colores.BrFuente = sbr;
			mMod.Herrs.Herr = mMod.Herrs.HerrPrev;							// Regresar a la herr prev
			break;
		case eHerr.Pincel:		m_priActual = new cPrimPincel(m_visPad, pt); break;
		case eHerr.Aerosol:		m_priActual = new cPrimPincel(m_visPad, pt); break;///
		case eHerr.Flood:		break;///
		case eHerr.Blur:		break;///
		case eHerr.Iluminar:	break;///
		case eHerr.Rect:		m_priActual = new cPrimRect(m_visPad, pt, false); break;
		case eHerr.RectRedond:	m_priActual = new cPrimRectRedond(m_visPad, pt); break;
		case eHerr.Elipse:		m_priActual = new cPrimRect(m_visPad, pt, true); break;
		case eHerr.Lin:			m_priActual = new cPrimLin(m_visPad, pt); break;
		case eHerr.Poli:		m_priActual = new cGeomPoli(m_visPad, pt); break;
		case eHerr.PoliLin:		m_priActual = new cGeomPoliLin(m_visPad, pt); break;
		case eHerr.Arc:			m_priActual = new cGeomArc(m_visPad, pt); break;
		case eHerr.Bezier:		m_priActual = new cGeomBezier(m_visPad, pt); break;
		case eHerr.QuadraticBezier:	m_priActual = new cGeomQuadraticBezier(m_visPad, pt); break;
	}
	// ** Empezar edición
	if (m_priActual != null)	{	m_visPad.m_undUndo.Add(m_priActual); m_visPad.Changed = true; mMod.Efectos.LimpiaEfectos();}
}
protected override void OnMouseMove(MouseArgs e)
{	Point pt;

	pt = e.Location / wMain.EscalaTot;	if (mMod.Cfg.Redondear)	pt.Truncate(); // Snap
	wMain.MuestraStatPos(string.Format(CultInf.CurrentCulture, "({0}, {1})", (int)pt.X, (int)pt.Y));
		if (e.Button != eMouseButton.Left || pt == m_ptAnt) return;	// Sin mov: salir
	m_ptAnt = pt;
	switch (mMod.Herrs.Herr)
	{	case eHerr.Sel: case eHerr.SelLibre:
			if (selCtlSel.ModoSel)	selCtlSel.OnMouseDrag(pt);
			break;
		default:
			if (m_priActual != null)	{	m_priActual.OnMouseDrag(pt); m_visPad.Changed = true;} // ** Agregar pt
			break;
	}
}
protected override void OnKeyDown(ref KeyArgs e)
{	switch (e.Key)
	{	case eKey.OemPlus:	mMod.MainWnd.Zoom(m_visPad, true);
			break;
		case eKey.OemMinus:	mMod.MainWnd.Zoom(m_visPad, false);
			break;
		default:
			if (m_priActual != null && e.Control)	{	m_priActual.OnPresCtrl(m_ptAnt); m_visPad.Changed = true;} // ** Agregar pt
			break;
	}
}
protected override void OnLostCapture()
{	if (selCtlSel.ModoSel)	selCtlSel.CompletaSel();					// Terminar sel (y empezar edición)
	m_priActual = null;													// Terminar forma
}
protected override void OnPaint(PaintArgs e)
{	e.Graphics.SetTransform(wMain.EscalaMatriz);							// Aplicar escala
	e.Graphics.FillRectangle(new Rectangle(Point.Zero, m_visPad.m_ptTam), mMod.FondoTransp); // ** Fondo
	m_visPad.m_undUndo.Pinta(e.Graphics);									// ** Pintar
	mMod.Efectos.Pinta3D(e);
	if (mMod.Cfg.Grid && wMain.EscalaTot.X >= 8)							// ** Grid
	{	float fAncho, fAlto, f;
	///sólo la parte visible
		fAncho = m_visPad.m_ptTam.X; fAlto = m_visPad.m_ptTam.Y; e.Graphics.Antialias = false;
		for (f = -0.5f; f < fAncho; f++)	e.Graphics.DrawVerticalLine(f, 0, fAlto, eBrush.Gray, 1, mMod.GridLin);
		for (f = -0.5f; f < fAlto; f++)		e.Graphics.DrawHorizontalLine(0, fAncho, f, eBrush.Gray, 1, mMod.GridLin);
	}
}
protected override void OnSizeChanged()						{	m_visPad.m_camCam.Size = Size;}
}
// ** cCtlSel -----------------------------------------------------------------------------------------------------------------------------
private class cCtlSel : cContainer
{// ** Tps
	enum eModo
	{	Ninguno	= 0,
		Sel,
		SelEf,
		Bitmap,
		Texto
	}
// ** Cpos
	fVista m_visPad;
	eModo m_modModo;
	Point m_ptIni; Rectangle m_rtPlaceSel, m_rtPlaceSelReal;
	cGeometry m_geoGeom; Wew.Array<Point> ma_ptGeom;						// ** Sel libre
	bool m_bBlqGeom; cTransformedGeometry m_tgEscalada;						// La geo escalada es para dibujar el focusrect sin transform
	dRedimEf m_reOnRedim;
	cTextBox txtTexto;
	cResizableBorder rzbBorde;
	public cBitmap Bitmap;
// ** Ctor/dtor
	public cCtlSel(fVista visPad)
	{	m_visPad = visPad;
		ma_ptGeom.MinimumCapacity = 100;
		txtTexto = new cTextBox() {	BackColor = null, BorderStyle = eBorderStyle.None, Margins = new Rect(0), Visible = false};
			AddControl(txtTexto);
		rzbBorde = new cResizableBorder() {	Control = this, Visible = false};
			rzbBorde.KeyDown += rzbBorde_KeyDown;
		HitTestTransparent = eHitTestTransparent.Background;
	}
	public void IniBorde(cContainer cntSuperfi)				{	cntSuperfi.AddControl(rzbBorde);}//¿agregar a la superficie (no al lienzo)
// ** Props
	public Rectangle PlaceSel								{	get	{	return m_rtPlaceSel;}} // Sel sin escala pero limitada a la img
	public Rectangle PlaceSelReal							{	get	{	return m_rtPlaceSelReal;}} // Sel sin escala y del tam total
	public bool PuedeMover
	{	set
		{	rzbBorde.HitTestTransparent = (value ? eHitTestTransparent.None : eHitTestTransparent.Background);
			rzbBorde.Cursor = (value ? mRes.CurMano : null);
		}
	}
	public bool ModoSel										{	get	{	return m_modModo == eModo.Sel;}}
	public bool ModoSelEf									{	get	{	return m_modModo == eModo.SelEf;}}
	public bool ModoTexto									{	get	{	return m_modModo == eModo.Texto;}}
	public cGeometry GeomSel
	{	get
		{		if (m_geoGeom == null)	return null;
			return new cTransformedGeometry(m_geoGeom, Matrix3x2.FromTranslation(m_rtPlaceSel.Location)); // Conv la geo a coords de lienzo
		}
		private set
		{	m_geoGeom?.Dispose(); m_geoGeom = value;
			m_tgEscalada?.Dispose(); m_tgEscalada = null;
		}
	}
// ** Mets
	public void IniConSel(Point ptPos, bool bLibre)
	{	TerminaEdición();													// Por precaución
		m_modModo = eModo.Sel; m_ptIni = ptPos;
		if (!bLibre)														// ** Usar rect
			Bounds = new Rectangle(ptPos, Point.Zero) * wMain.EscalaTot;
		else																// ** Usar poli
		{	Bounds = new Rectangle(Point.Zero, m_visPad.m_ptTam) * wMain.EscalaTot; // Inicialmente, tomar todo el lienzo
			m_geoGeom = new cPathGeometry((cPathGeometry.Sink s) => {}); ma_ptGeom.Clear(); // ** Crear geom
		}
		Show();
	}
	public void IniConSelEf(Rectangle rtRegión, dRedimEf reOnRedim)
	{	if (m_modModo != eModo.SelEf)										// No perder sel
		{	TerminaEdición();
			m_modModo = eModo.SelEf; Bounds = rtRegión * wMain.EscalaTot; Show();
			PuedeMover = true; rzbBorde.Show(); rzbBorde.Focus();
		}
		m_reOnRedim = reOnRedim; reOnRedim();								// Aplicar región actual desde ahora
	}
	public void IniConBmp(Rectangle rtPlace, cBitmap bmpBmp)
	{	m_modModo = eModo.Bitmap; Bounds = rtPlace * wMain.EscalaTot; Bitmap = bmpBmp; Show();
		PuedeMover = true; rzbBorde.Show(); rzbBorde.Focus();
	}
	public void IniConTxt(Point ptPos)
	{	m_modModo = eModo.Texto; Bounds = new Rectangle(ptPos, new Point(100, 20)) * wMain.EscalaTot; Show();
		txtTexto.Text = null; txtTexto.Show(); ActualizaTxt(); txtTexto.Focus();
		PuedeMover = false; rzbBorde.Show();
	}
	public void OnMouseDrag(Point ptPos)
	{		if (m_modModo != eModo.Sel)	return;
		if (m_geoGeom == null)												// ** Rect
		{	Rectangle rt;

			rt = new Rectangle(m_ptIni, ptPos - m_ptIni);
			if (rt.Width < 0)	{	rt.X = rt.Right; rt.Width = -rt.Width;} // Ordenar pts
			if (rt.Height < 0)	{	rt.Y = rt.Bottom; rt.Height = -rt.Height;}
			Bounds = rt * wMain.EscalaTot;
		} else																// ** Libre
		{	GeomSel = new cPathGeometry((cPathGeometry.Sink pgsSink) =>		// Crear nuevo poli
				{	pgsSink.BeginFigure(m_ptIni, true);
						ma_ptGeom.Add(ptPos); pgsSink.AddLines(ma_ptGeom.Data, 0, ma_ptGeom.Count);
					pgsSink.EndFigure(true);
				});
			Invalidate();
		}
	}
	public void CompletaSel()
	{		if (m_modModo != eModo.Sel)	return;
		if (m_geoGeom != null)												// ** Libre
		{	Rectangle rt;

			rt = m_geoGeom.GetBounds();	if (float.IsInfinity(rt.Width))	{	TerminaEdición(); return;} // Sel vacía: terminar sel, salir
			m_bBlqGeom = true; Bounds = rt * wMain.EscalaTot; m_bBlqGeom = false; // Rodear la sel
			if (rt.Location != Point.Zero)									// Conv la geo a coords de ctlsel
				GeomSel = new cTransformedGeometry(m_geoGeom, Matrix3x2.FromTranslation(-rt.Location));
		}
		if (Size.GetMinMember().X <= 1)	{	TerminaEdición(); return;}		// Sel muy pequeña: terminar sel, salir
		PuedeMover = false; rzbBorde.Show(); rzbBorde.Focus();
	}
	public void AplicaEscala()
	{	m_bBlqGeom = true; Bounds = PlaceSel * wMain.EscalaTot; m_bBlqGeom = false; // ** Redim
		if (m_tgEscalada != null)	{	m_tgEscalada.Dispose(); m_tgEscalada = null;}
		ActualizaTxt();
	}
	public void InvSel()
	{		if (m_modModo != eModo.Sel)	return;
		using (cGeometry geoSel = (m_geoGeom != null ? GeomSel : new cRectangleGeometry(m_rtPlaceSel))) // ** Si no hay sel libre: tomar la sel rect
		{	GeomSel = InvSel(geoSel);										// Inv sel
			CompletaSel();													// Conv la geo a coords de ctlsel
		}
		Invalidate();
	}
	public cPathGeometry InvSel(cGeometry geoSel)
	{	cPathGeometry pgRes;

		using (cRectangleGeometry rgTodo = new cRectangleGeometry(new Rectangle(Point.Zero, m_visPad.m_ptTam)))
		{	pgRes = new cPathGeometry((cPathGeometry.Sink pgsSink) =>
				pgsSink.AddCombinedGeometries(rgTodo, geoSel, cPathGeometry.eCombine.Exclude));
		}
		return pgRes;
	}
	public void ActualizaTxt()
	{		if (m_modModo != eModo.Texto)	return;
		txtTexto.Font = new cFont(mMod.Colores.Fuente.Name, mMod.Colores.Fuente.Size * wMain.EscalaTot.X); // Aplicar fnt y escala
		txtTexto.TextColor = mMod.Colores.BrFuente;
	}
	public void Borra()
	{	switch (m_modModo)
		{	case eModo.Sel:													// ** Hay sel: borrar sel
				if (m_geoGeom == null)										// Rect
					m_visPad.m_undUndo.Add(new cPrimRect(m_visPad, m_rtPlaceSel, false));
				else														// Libre
				{	m_visPad.m_undUndo.Add(new cGeomPoli(m_visPad, GeomSel)); m_geoGeom = null; // No hacer dispose porque la geom se compartió
				}
				m_visPad.Changed = true;
				break;
		}
		m_Cancela();
	}
	public void TerminaEdición()
	{	switch (m_modModo)
		{	case eModo.Bitmap:	m_visPad.m_undUndo.Add(new cPrimBmp(m_visPad, Bitmap, m_rtPlaceSelReal)); m_visPad.Changed = true;
				break;
			case eModo.Texto:	m_visPad.m_undUndo.Add(new cPrimTxt(m_visPad, m_rtPlaceSelReal, txtTexto.Text)); m_visPad.Changed = true;
				break;
		}
		m_Cancela();
	}
	protected override void OnVisibleChanged()
	{	if (Visible)														// Mostrar coords
		{	RectangleI rti;
		
			rti = m_rtPlaceSel.GetExpandedToInteger();
			wMain.MuestraStatPos(string.Format(CultInf.CurrentCulture, "({0}, {1}) - ({2}, {3})", rti.X, rti.Y, rti.Right, rti.Bottom));
			wMain.MuestraStatPos(string.Format(CultInf.CurrentCulture, "{0} x {1}", rti.Width, rti.Height));
		} else
		{	wMain.MuestraStatPos(null); wMain.MuestraStatPos(null);
		}	
	}
	protected override void OnBoundsChanged()
	{	Rectangle rtSelNueva;

		rtSelNueva = Bounds / wMain.EscalaTot; m_rtPlaceSelReal = rtSelNueva;
		rtSelNueva.Intersect(new Rectangle(Point.Zero, m_visPad.m_ptTam));
		if (m_geoGeom != null && !m_bBlqGeom)								// El usr está ajustando el tam: redim geo
		{	GeomSel = new cTransformedGeometry(m_geoGeom, Matrix3x2.FromScale(rtSelNueva.Size / m_rtPlaceSel.Size));
		}
		m_rtPlaceSel = rtSelNueva;
		rzbBorde.UpdateBounds();
		OnVisibleChanged();													// Mostrar coords
		if (m_modModo == eModo.SelEf)	m_reOnRedim?.Invoke();				// ** Seleccionando región de ef: actualizar ef
	}
	protected override void OnPaint(PaintArgs e)
	{	Rectangle rt = ClientRectangle;

		if (Bitmap != null)
			e.Graphics.DrawBitmap(Bitmap, rt, eAlignment.None, eResize.Stretch, 1, eInterpolation.NearestNeighbor);
		else if (m_geoGeom != null)
		{	if (m_tgEscalada == null)	m_tgEscalada = new cTransformedGeometry(m_geoGeom, wMain.EscalaMatriz); // ** Crear geom escalada
			e.Graphics.Antialias = false; e.Graphics.DrawGeometry(m_tgEscalada, eBrush.SystemFocusRectangle);
		} else if (!rzbBorde.Visible)
			e.Graphics.DrawFocusRectangle(rt);
	}
	protected override void OnKeyDownRouted(ref KeyArgs e, bool IsPreview)
	{		if (!IsPreview)	return;											// Sólo antes de notif: salir
		if (e.Key == eKey.Escape)	rzbBorde_KeyDown(null, ref e);			// Escape en texto
	}
	private void rzbBorde_KeyDown(object sender, ref KeyArgs e)
	{	switch (e.Key)
		{	case eKey.Escape:												// ** Cancelar
				m_Cancela(); e.Handled = true;
				break;
			case eKey.Delete:												// ** Eliminar o cancelar
				Borra(); e.Handled = true;
				break;
			case eKey.Enter:												// ** Aplicar
				if (m_modModo == eModo.Bitmap)	{	TerminaEdición(); e.Handled = true;}
				break;
		}
	}
	private void m_Cancela()
	{	Bitmap = null; txtTexto.Hide(); GeomSel = null; m_reOnRedim = null;
		if (m_modModo == eModo.SelEf)	mMod.Efectos.LimpiaEfectos();
		m_modModo = eModo.Ninguno; rzbBorde.Hide();
			if (!Visible)	return;											// Ya cancelado: evitar llamada recur al cambiar de err; salir
		Hide();	if (mMod.Herrs.Herr == eHerr.Mano)	mMod.Herrs.Herr = mMod.Herrs.HerrPrev;
	}
}
	}
}