using Wew.Control;
using Wew.Media;

namespace DirectPaint
{
class kColores : cDockControl
{	cFont m_fntFuente;
	uBrush ubrBorde, ubrRelleno, ubrFuente, ubrFondo; cButton btnFuente;
public kColores()
{	cTabControl tc; cTabControl.cTab tab; cScrollableControl sc; cLabel lbl;
	
	Width = 320; IsChildForm = false; Text = "Colors and font"; HideOnClose = true; Dock = eDirection.Left;
	tc = new cTabControl() {	Margins = new Rect(0)};
		tc.LayoutSuspended = true; tc.Header.LayoutSuspended = true;
	// ** Brd
	tab = tc.Tabs.Add("Border");
	sc = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; sc.ClientArea.SuspendLayout();
		ubrBorde = new uBrush() {	RightMargin = 1};
			sc.ClientArea.AddControl(ubrBorde);
		sc.ClientArea.LayoutSuspended = false; tab.Content = sc;
	// ** Relle
	tab = tc.Tabs.Add("Fill");
	sc = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; sc.ClientArea.SuspendLayout();
		ubrRelleno = new uBrush() {	RightMargin = 1};
			sc.ClientArea.AddControl(ubrRelleno);
		sc.ClientArea.LayoutSuspended = false; tab.Content = sc;
	// ** Fnt
	tab = tc.Tabs.Add("Font");
	sc = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; sc.ClientArea.SuspendLayout();
		ubrFuente = new uBrush() {	RightMargin = 1}; ubrFuente.Changed += ubrFuente_Changed;
			sc.ClientArea.AddControl(ubrFuente);
		lbl = new cLabel() {	Text = "Font", Font = eFont.SystemBoldText, TopMargin = 347};
			sc.ClientArea.AddControl(lbl);
		btnFuente = new cButton() {	Text = "Text", Width = 200, Height = 60, LeftMargin = 10, TopMargin = 370};
			btnFuente.Click += btnFuente_Click; sc.ClientArea.AddControl(btnFuente);
		sc.ClientArea.LayoutSuspended = false; tab.Content = sc;
	// ** Fondo
	tab = tc.Tabs.Add("Background");
	sc = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; sc.ClientArea.SuspendLayout();
		ubrFondo = new uBrush() {	RightMargin = 1}; ubrFondo.Changed += ubrFondo_Changed;
			sc.ClientArea.AddControl(ubrFondo);
		sc.ClientArea.LayoutSuspended = false; tab.Content = sc;
	tc.LayoutSuspended = false; tc.Header.LayoutSuspended = false;
	AddControl(tc);
	// ** Cfg
	ubrFondo.SuspendLayout();
		if (!string.IsNullOrEmpty(Settings.Default.BrFondo))
			ubrFondo.Load(Settings.Default.BrFondo);
		else
		{	ubrFondo.Value = eBrush.White;
		}
	ubrFondo.LayoutSuspended = false;
	ubrRelleno.SuspendLayout();
		if (!string.IsNullOrEmpty(Settings.Default.BrRelleno))
			ubrRelleno.Load(Settings.Default.BrRelleno);
		else
		{	ubrRelleno.Value = eBrush.White;
		}
	ubrRelleno.LayoutSuspended = false;
	ubrBorde.SuspendLayout();
		if (!string.IsNullOrEmpty(Settings.Default.BrBorde))
			ubrBorde.Load(Settings.Default.BrBorde);
		else
		{	ubrBorde.Value = eBrush.Black;
		}
	ubrBorde.LayoutSuspended = false;
	ubrFuente.SuspendLayout();
		if (!string.IsNullOrEmpty(Settings.Default.BrFuente))
			ubrFuente.Load(Settings.Default.BrFuente);
		else
		{	ubrFuente.Value = eBrush.Black;
		}
	ubrFuente.LayoutSuspended = false;
	btnFuente.Font = m_fntFuente = new cFont(Settings.Default.FntNomb, Settings.Default.FntTam
		, Settings.Default.FntWeight, Settings.Default.FntEstilo, Settings.Default.FntStretch);
}
public void GrabaCfg()
{	Settings.Default.BrFondo = ubrFondo.Save();
	Settings.Default.BrRelleno = ubrRelleno.Save();
	Settings.Default.BrBorde = ubrBorde.Save();
	Settings.Default.BrFuente = ubrFuente.Save();
	Settings.Default.FntNomb = m_fntFuente.Name; Settings.Default.FntTam = m_fntFuente.Size;
		Settings.Default.FntWeight = m_fntFuente.Weight; Settings.Default.FntEstilo = m_fntFuente.Style;
		Settings.Default.FntStretch = m_fntFuente.Stretch;
}
public cBrush Relleno
{	get														{	return ubrRelleno.Value;}
	set														{	ubrRelleno.Value = value;} // Establecido al tomar muestra
}
public cBrush Borde
{	get														{	return ubrBorde.Value;}
	set														{	ubrBorde.Value = value;} // Establecido al tomar muestra
}
public cBrush BrFuente
{	get														{	return ubrFuente.Value;}
	set														{	ubrFuente.Value = value;} // Establecido al tomar muestra
}
public cFont Fuente											{	get	{	return m_fntFuente;}}
public void OnVistaEnfocada(fVista visVista)				{	ubrFondo.Value = visVista.Fondo;}
private void ubrFuente_Changed(object sender)				{	wMain.VistaActual?.ActualizaTxt();}
private void btnFuente_Click(object sender)
{	uFont fc = new uFont();

	fc.Value = m_fntFuente;	if (!fc.ShowDialog())	return;
	btnFuente.Font = m_fntFuente = fc.Value;
	wMain.VistaActual?.ActualizaTxt();
}
private void ubrFondo_Changed(object sender)
{	fVista vis = wMain.VistaActual;

	if (vis != null)	{	vis.Fondo = ubrFondo.Value; vis.Changed = true;}
}
}
}