using Wew.Control;
using Wew.Media;

namespace DirectPaint;

sealed class kColores : cDockControl
{	cFont m_fntFuente;
	readonly uBrush ubrBorde, ubrRelleno, ubrFuente, ubrFondo; readonly cButton btnFuente;
public kColores()
{	cTabControl tc; cTabControl.cTab tab; cScrollableControl sc; cLabel lbl;
	
	Width = 320; IsForm = false; Text = "Colors and font"; HideOnClose = true; Dock = eDirection.Left;
	tc = new cTabControl {	Margins = new Rect(0)};
	// ** Brd
	tab = tc.Tabs.Add("Border");
	sc = new cScrollableControl(); sc.VerticalBar.Visible = true;
		ubrBorde = new uBrush {	RightMargin = 1};
			sc.ClientArea.AddControl(ubrBorde);
		tab.Content = sc;
	// ** Relle
	tab = tc.Tabs.Add("Fill");
	sc = new cScrollableControl(); sc.VerticalBar.Visible = true;
		ubrRelleno = new uBrush {	RightMargin = 1};
			sc.ClientArea.AddControl(ubrRelleno);
		tab.Content = sc;
	// ** Fnt
	tab = tc.Tabs.Add("Font");
	sc = new cScrollableControl(); sc.VerticalBar.Visible = true;
		ubrFuente = new uBrush {	RightMargin = 1}; ubrFuente.Changed += ubrFuente_Changed;
			sc.ClientArea.AddControl(ubrFuente);
		lbl = new cLabel {	Text = "Font", Font = eFont.SystemBoldText, TopMargin = 462};
			sc.ClientArea.AddControl(lbl);
		btnFuente = new cButton {	Text = "Text", Width = 200, Height = 60, LeftMargin = 10, TopMargin = 485};
			btnFuente.Click += btnFuente_Click; sc.ClientArea.AddControl(btnFuente);
		tab.Content = sc;
	// ** Fondo
	tab = tc.Tabs.Add("Background");
	sc = new cScrollableControl(); sc.VerticalBar.Visible = true;
		ubrFondo = new uBrush {	RightMargin = 1}; ubrFondo.Changed += ubrFondo_Changed;
			sc.ClientArea.AddControl(ubrFondo);
		tab.Content = sc;
	AddControl(tc);
	// ** Cfg
	if (!string.IsNullOrEmpty(Settings.Default.BrFondo))
		ubrFondo.Load(Settings.Default.BrFondo);
	else
	{	ubrFondo.Value = eBrush.White;
	}
	if (!string.IsNullOrEmpty(Settings.Default.BrRelleno))
		ubrRelleno.Load(Settings.Default.BrRelleno);
	else
	{	ubrRelleno.Value = eBrush.White;
	}
	if (!string.IsNullOrEmpty(Settings.Default.BrBorde))
		ubrBorde.Load(Settings.Default.BrBorde);
	else
	{	ubrBorde.Value = eBrush.Black;
	}
	if (!string.IsNullOrEmpty(Settings.Default.BrFuente))
		ubrFuente.Load(Settings.Default.BrFuente);
	else
	{	ubrFuente.Value = eBrush.Black;
	}
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
{	get														=>	ubrRelleno.Value;
	set														=>	ubrRelleno.Value = value; // Establecido al tomar muestra
}
public cBrush Borde
{	get														=>	ubrBorde.Value;
	set														=>	ubrBorde.Value = value; // Establecido al tomar muestra
}
public cBrush BrFuente
{	get														=>	ubrFuente.Value;
	set														=>	ubrFuente.Value = value; // Establecido al tomar muestra
}
public cFont Fuente											=>	m_fntFuente;
public void OnVistaEnfocada(fVista visVista)				{	ubrFondo.Value = visVista.Fondo;}
private void ubrFuente_Changed(object sender)				{	wMain.VistaActual?.ActualizaTxt();}
private void btnFuente_Click(object sender)
{	uFont fc;

	fc = new uFont {	Value = m_fntFuente};	if (!fc.ShowDialog())	return;
	btnFuente.Font = m_fntFuente = fc.Value;
	wMain.VistaActual?.ActualizaTxt();
}
private void ubrFondo_Changed(object sender)
{	fVista vis = wMain.VistaActual;

	if (vis is not null)	{	vis.Fondo = ubrFondo.Value; vis.Changed = true;}
}
}