using Cursor = System.Windows.Forms.Cursor;
using Wew.Control;
using Wew.Media;

namespace DirectPaint
{
class kHerrs : cDockControl
{	eHerr m_herHerr, m_herPrev;
	Cursor m_curCursor;
	cToolButton tbbSel, tbbSelLibre, tbbInvSel, tbbMano, tbbTexto, tbbMuestra, tbbPincel, tbbAerosol, tbbFlood, tbbBlur, tbbIluminar;
	cToolButton tbbRect, tbbElipse, tbbRectRedond, tbbLin, tbbPoliLin, tbbPoli, tbbArc, tbbBezier, tbbQuadraticBezier;
	cToolButton tbbBorrador;
	cToolButton tbbAncho1, tbbAncho10, tbbAncho20, tbbAncho40, tbbBorde, tbbRellenar, tbbCerrado, tbbRecortar, tbbRotarIzq, tbbRotarDer;
// ** Ctor/dtor
public kHerrs()
{	cWrapPanel wpCli; cStackPanel sp, sp2;

	Width = 78; IsChildForm = false; Text = "Tools"; HideOnClose = true; Dock = eDirection.Left;
	BorderStyle = eBorderStyle.Default; Border = new Rect(2);
	wpCli = new cWrapPanel() {	Margins = new Rect(0)};
		tbbSel = new cToolButton() {	Bitmap = mRes.BmpSel, ToolTip = "Select", Type = eButtonType.Radio};
			tbbSel.Click += tbbSel_Click; wpCli.AddControl(tbbSel);
		tbbSelLibre = new cToolButton() {	Bitmap = mRes.BmpSelLibre, ToolTip = "Free selection", Type = eButtonType.Radio};
			tbbSelLibre.Click += tbbSelLibre_Click; wpCli.AddControl(tbbSelLibre);
		tbbInvSel = new cToolButton() {	Bitmap = mRes.BmpInvSel, ToolTip = "Invert selection"};
			tbbInvSel.Click += tbbInvSel_Click; wpCli.AddControl(tbbInvSel);
		tbbMano = new cToolButton() {	Bitmap = mRes.BmpMano, ToolTip = "Move", Type = eButtonType.Radio};
			tbbMano.Click += tbbMano_Click; wpCli.AddControl(tbbMano);
		tbbTexto = new cToolButton() {	Bitmap = mRes.BmpTexto, ToolTip = "Text", Type = eButtonType.Radio};
			tbbTexto.Click += tbbTexto_Click; wpCli.AddControl(tbbTexto);
		tbbMuestra = new cToolButton() {	Bitmap = mRes.BmpMuestra, ToolTip = "Pick color (Ctrl+K)", Type = eButtonType.Radio};
			tbbMuestra.Click += tbbMuestra_Click; wpCli.AddControl(tbbMuestra);
		wpCli.AddControl(new cSeparator());
		tbbPincel = new cToolButton() {	Bitmap = mRes.BmpPincel, ToolTip = "Brush", Type = eButtonType.Radio};
			tbbPincel.Click += tbbPincel_Click; wpCli.AddControl(tbbPincel);
		tbbAerosol = new cToolButton() {	Bitmap = mRes.BmpAerosol, ToolTip = "Spray", Type = eButtonType.Radio};
			tbbAerosol.Click += tbbAerosol_Click; wpCli.AddControl(tbbAerosol);
		tbbFlood = new cToolButton() {	Bitmap = mRes.BmpFlood, ToolTip = "Flood", Type = eButtonType.Radio};
			//tbbFlood.Click += tbbFlood_Click; wpCli.AddControl(tbbFlood);
		tbbBlur = new cToolButton() {	Bitmap = mRes.BmpFlood, ToolTip = "Blur", Type = eButtonType.Radio};
			//tbbBlur.Click += tbbBlur_Click; wpCli.AddControl(tbbBlur);
		tbbIluminar = new cToolButton() {	Bitmap = mRes.BmpFlood, ToolTip = "Highlight", Type = eButtonType.Radio};
			//tbbIluminar.Click += tbbIluminar_Click; wpCli.AddControl(tbbIluminar);
		tbbRect = new cToolButton() {	Bitmap = mRes.BmpRect, ToolTip = "Rectangle", Type = eButtonType.Radio};
			tbbRect.Click += tbbRect_Click; wpCli.AddControl(tbbRect);
		tbbElipse = new cToolButton() {	Bitmap = mRes.BmpElipse, ToolTip = "Ellipse", Type = eButtonType.Radio};
			tbbElipse.Click += tbbElipse_Click; wpCli.AddControl(tbbElipse);
		tbbRectRedond = new cToolButton() {	Bitmap = mRes.BmpRectRedond, ToolTip = "Rounded rectangle", Type = eButtonType.Radio};
			tbbRectRedond.Click += tbbRectRedond_Click; wpCli.AddControl(tbbRectRedond);
		tbbLin = new cToolButton() {	Bitmap = mRes.BmpLin, ToolTip = "Line", Type = eButtonType.Radio};
			tbbLin.Click += tbbLin_Click; wpCli.AddControl(tbbLin);
		tbbPoliLin = new cToolButton() {	Bitmap = mRes.BmpPoliLin
				, ToolTip = "Polyline (press Ctrl to add points)", Type = eButtonType.Radio};
			tbbPoliLin.Click += tbbPoliLin_Click; wpCli.AddControl(tbbPoliLin);
		tbbPoli = new cToolButton() {	Bitmap = mRes.BmpPoli, ToolTip = "Polygon", Type = eButtonType.Radio};
			tbbPoli.Click += tbbPoli_Click; wpCli.AddControl(tbbPoli);
		tbbArc = new cToolButton() {	Bitmap = mRes.BmpArc, ToolTip = "Arc", Type = eButtonType.Radio};
			tbbArc.Click += tbbArc_Click; wpCli.AddControl(tbbArc);
		tbbBezier = new cToolButton() {	Bitmap = mRes.BmpBezier, ToolTip = "Bezier (press Ctrl to add points)", Type = eButtonType.Radio};
			tbbBezier.Click += tbbBezier_Click; wpCli.AddControl(tbbBezier);
		tbbQuadraticBezier = new cToolButton() {	Bitmap = mRes.BmpQuadraticBezier
				, ToolTip = "Quadratic bezier (press Ctrl to add points)", Type = eButtonType.Radio};
			tbbQuadraticBezier.Click += tbbQuadraticBezier_Click; wpCli.AddControl(tbbQuadraticBezier);
		wpCli.AddControl(new cSeparator());
		tbbBorrador = new cToolButton() {	Bitmap = mRes.BmpBorrador, ToolTip = "Erase", Type = eButtonType.Check};
			tbbBorrador.Click += tbbBorrar_Click;
			wpCli.AddControl(tbbBorrador);
		wpCli.AddControl(new cSeparator());
		sp2 = new cStackPanel() {	AutoSize = eAutoSize.Both};
			sp = new cStackPanel() {	AutoSize = eAutoSize.Both, Direction = eDirection.Bottom};
				tbbAncho1 = new cToolButton() {	Bitmap = mRes.BmpAncho1, ToolTip = "1 pixel", Type = eButtonType.Radio, Height = 24};
					tbbAncho1.Click += tbbAncho1_Click; sp.AddControl(tbbAncho1);
				tbbAncho10 = new cToolButton() {	Bitmap = mRes.BmpAncho2, ToolTip = "10 pixel", Type = eButtonType.Radio, Height = 24};
					tbbAncho10.Click += tbbAncho10_Click; sp.AddControl(tbbAncho10);
				tbbAncho20 = new cToolButton() {	Bitmap = mRes.BmpAncho3, ToolTip = "20 pixel", Type = eButtonType.Radio, Height = 24};
					tbbAncho20.Click += tbbAncho20_Click; sp.AddControl(tbbAncho20);
				tbbAncho40 = new cToolButton() {	Bitmap = mRes.BmpAncho4, ToolTip = "40 pixel", Type = eButtonType.Radio, Height = 24};
					tbbAncho40.Click += tbbAncho40_Click; sp.AddControl(tbbAncho40);
				sp2.AddControl(sp);
			sp = new cStackPanel() {	AutoSize = eAutoSize.Both, Direction = eDirection.Bottom};
				tbbBorde = new cToolButton() {	Bitmap = mRes.BmpBorde, ToolTip = "Border", Type = eButtonType.Check};
					tbbBorde.Click += tbbBorde_Click; sp.AddControl(tbbBorde);
				tbbRellenar = new cToolButton() {	Bitmap = mRes.BmpRelleno, ToolTip = "Fill", Type = eButtonType.Check};
					tbbRellenar.Click += tbbRellenar_Click; sp.AddControl(tbbRellenar);
				tbbCerrado = new cToolButton() {	Bitmap = mRes.BmpCerrado, ToolTip = "Closed", Type = eButtonType.Check};
					tbbCerrado.Click += tbbCerrado_Click; sp.AddControl(tbbCerrado);
				sp2.AddControl(sp);
			wpCli.AddControl(sp2);
		wpCli.AddControl(new cSeparator());
		tbbRecortar = new cToolButton() {	Bitmap = mRes.BmpRecortar, ToolTip = "Clip"};
			tbbRecortar.Click += tbbRecortar_Click; wpCli.AddControl(tbbRecortar);
		tbbRotarIzq = new cToolButton() {	Bitmap = mRes.BmpRotarIzq, ToolTip = "Rotate left 90°"};
			tbbRotarIzq.Click += tbbRotarIzq_Click; wpCli.AddControl(tbbRotarIzq);
		tbbRotarDer = new cToolButton() {	Bitmap = mRes.BmpRotarDer, ToolTip = "Rotate right 90°"};
			tbbRotarDer.Click += tbbRotarDer_Click; wpCli.AddControl(tbbRotarDer);
	AddControl(wpCli);
}
public void Ini()
{	m_herHerr = eHerr.Sel - 1; Herr = (eHerr)Settings.Default.Herr;
	if (Settings.Default.Borde)       tbbBorde.PerformClick();
	if (Settings.Default.Cerrado)     tbbCerrado.PerformClick();
	if (Settings.Default.Rellenar)    tbbRellenar.PerformClick();
	if (Settings.Default.Borrar)      tbbBorrador.PerformClick();
}
public void GrabaCfg()
{	Settings.Default.Herr = (int)m_herHerr;
	Settings.Default.Borde = tbbBorde.Checked; Settings.Default.Cerrado = tbbCerrado.Checked;
	Settings.Default.Rellenar = tbbRellenar.Checked; Settings.Default.Borrar = tbbBorrador.Checked;
}
// ** Props
public eHerr Herr
{	get														{	return m_herHerr;}
	set
	{		if (value == m_herHerr)	return;									// Sin cambio: no perder herr prev, salir
		switch (value)
		{	case eHerr.SelLibre:	tbbSelLibre.PerformClick(); break;
			case eHerr.Mano:		tbbMano.PerformClick(); break;
			case eHerr.Texto:		tbbTexto.PerformClick(); break;
			case eHerr.Muestra:		tbbMuestra.PerformClick(); break;
			case eHerr.Pincel:		tbbPincel.PerformClick(); break;
			case eHerr.Aerosol:		tbbAerosol.PerformClick(); break;
			case eHerr.Flood:		tbbFlood.PerformClick(); break;
			case eHerr.Blur:		tbbBlur.PerformClick(); break;
			case eHerr.Iluminar:	tbbIluminar.PerformClick(); break;
			case eHerr.Rect:		tbbRect.PerformClick(); break;
			case eHerr.RectRedond:	tbbRectRedond.PerformClick(); break;
			case eHerr.Elipse:		tbbElipse.PerformClick(); break;
			case eHerr.Lin:			tbbLin.PerformClick(); break;
			case eHerr.Poli:		tbbPoli.PerformClick(); break;
			case eHerr.PoliLin:		tbbPoliLin.PerformClick(); break;
			case eHerr.Arc:			tbbArc.PerformClick(); break;
			case eHerr.Bezier:		tbbBezier.PerformClick(); break;
			default:				tbbSel.PerformClick(); break;
		}
	}
}
public eHerr HerrPrev										{	get	{	return m_herPrev;}}
public bool Borrar											{	get	{	return tbbBorrador.Checked;}}
public bool Borde											{	get	{	return tbbBorde.Checked;}}
public bool Rellenar										{	get	{	return tbbRellenar.Checked;}}
public bool Cerrado											{	get	{	return tbbCerrado.Checked;}}
// ** Mets
public void RefrescaTam()
{	int i = (int)mMod.Cfg.AnchoLin;

	tbbAncho1.Checked = false; tbbAncho10.Checked = false; tbbAncho20.Checked = false; tbbAncho40.Checked = false;
		if (i != mMod.Cfg.AnchoLin)	return;								// No es cifra redonda: salir
	switch (i)
	{	case 1:		tbbAncho1.Checked = true; break;
		case 10:	tbbAncho10.Checked = true; break;
		case 20:	tbbAncho20.Checked = true; break;
		case 40:	tbbAncho40.Checked = true; break;
	}
}
public void OnVistaEnfocada(fVista visVista)				{	visVista.SetCursor(m_curCursor);}
private void tbbSel_Click(object sender)					{	m_SelHerr(eHerr.Sel, mRes.CurSel);}
private void tbbSelLibre_Click(object sender)				{	m_SelHerr(eHerr.SelLibre, mRes.CurSelLibre);}
private void tbbInvSel_Click(object sender)					{	if (wMain.VistaActual?.ModoSel == true)	wMain.VistaActual.InvSel();}
private void tbbMano_Click(object sender)
{	m_SelHerr(eHerr.Mano, mRes.CurMano, false);	if (wMain.VistaActual != null)	wMain.VistaActual.PuedeMover = true;
}
private void tbbTexto_Click(object sender)
{	m_SelHerr(eHerr.Texto, mRes.CurTexto, (wMain.VistaActual?.ModoTexto == false)); // Si está en modo texto (herr es texto o mano) se continua la edición
}
private void tbbMuestra_Click(object sender)				{	m_SelHerr(eHerr.Muestra, mRes.CurMuestra);}
private void tbbPincel_Click(object sender)					{	m_SelHerr(eHerr.Pincel, mRes.CurPincel);}
private void tbbAerosol_Click(object sender)				{	m_SelHerr(eHerr.Aerosol, mRes.CurAerosol);}
private void tbbFlood_Click(object sender)					{	m_SelHerr(eHerr.Flood, mRes.CurFlood);}
private void tbbBlur_Click(object sender)					{	m_SelHerr(eHerr.Blur, mRes.CurBlur);}
private void tbbIluminar_Click(object sender)				{	m_SelHerr(eHerr.Iluminar, mRes.CurIluminar);}
private void tbbRect_Click(object sender)					{	m_SelHerr(eHerr.Rect, mRes.CurRect);}
private void tbbRectRedond_Click(object sender)				{	m_SelHerr(eHerr.RectRedond, mRes.CurRectRedond);}
private void tbbElipse_Click(object sender)					{	m_SelHerr(eHerr.Elipse, mRes.CurElipse);}
private void tbbLin_Click(object sender)					{	m_SelHerr(eHerr.Lin, mRes.CurLin);}
private void tbbPoli_Click(object sender)					{	m_SelHerr(eHerr.Poli, mRes.CurPoli);}
private void tbbPoliLin_Click(object sender)				{	m_SelHerr(eHerr.PoliLin, mRes.CurPoliLin);}
private void tbbArc_Click(object sender)					{	m_SelHerr(eHerr.Arc, mRes.CurArc);}
private void tbbBezier_Click(object sender)					{	m_SelHerr(eHerr.Bezier, mRes.CurBezier);}
private void tbbQuadraticBezier_Click(object sender)		{	m_SelHerr(eHerr.QuadraticBezier, mRes.CurQuadraticBezier);}
private void tbbAncho1_Click(object sender)					{	mMod.Cfg.AnchoLin = 1;}
private void tbbAncho10_Click(object sender)				{	mMod.Cfg.AnchoLin = 10;}
private void tbbAncho20_Click(object sender)				{	mMod.Cfg.AnchoLin = 20;}
private void tbbAncho40_Click(object sender)				{	mMod.Cfg.AnchoLin = 40;}
private void tbbBorrar_Click(object sender)					{	wMain.TerminaEdición();}
private void tbbBorde_Click(object sender)					{	wMain.TerminaEdición();}
private void tbbCerrado_Click(object sender)				{	wMain.TerminaEdición();}
private void tbbRellenar_Click(object sender)				{	wMain.TerminaEdición();}
private void tbbRecortar_Click(object sender)
{	Rectangle rtSel; fVista vis = wMain.VistaActual;

		if (vis?.ModoSel != true)	return;									// Sin sel: salir
	rtSel = vis.PlaceSel; vis.Tam = rtSel.Size; mMod.Efectos.Recorta(rtSel); // ** Redim
}
private void tbbRotarIzq_Click(object sender)				{	mMod.Efectos.Rota(-90, true);}
private void tbbRotarDer_Click(object sender)				{	mMod.Efectos.Rota(90, true);}
// ** Mets
void m_SelHerr(eHerr herHer, Cursor curCursor, bool bTerminarEdic = true)
{	if (bTerminarEdic)	wMain.TerminaEdición();
	m_herPrev = m_herHerr; m_herHerr = herHer; m_curCursor = curCursor; wMain.VistaActual?.SetCursor(curCursor);
}}
}