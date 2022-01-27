using Wew.Control;
using Wew.Media;

namespace DirectPaint
{
class kCfg : cDockControl
{	Point m_ptTam;
	cNumericTextBox ntbAncho, ntbAlto, ntbDura; cCheckBox chkEstirar;
	cComboBox cboDdsFmt; cCheckBox chkCubo;
	uLineStyle ulsTrazo; cNumericTextBox ntbAnchoLin;
	uPoint uptRrEsquina, uptArcRadio; uSlider uslArcAng; cCheckBox chkRedondear, chkGrid, chkAntialias, chkSnap;
public kCfg()
{	cTabControl tc; cTabControl.cTab tab; cScrollableControl scCli;
	cStackPanel spnCli; uPropertyGroup grp; uPropertySubgroup sgr; cContainer cnt; cButton btn;

	Width = 310; Text = "Settings"; IsChildForm = false; HideOnClose = true; Dock = eDirection.Left;
	tc = new cTabControl() {	Margins = new Rect(0)}; tc.LayoutSuspended = true; tc.Header.LayoutSuspended = true;
	// ** Fme
	tab = tc.Tabs.Add("Frame");
	scCli = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; scCli.ClientArea.SuspendLayout();
	spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup() {	Text = "Size"};
		sgr = new uPropertySubgroup() {	Text = "Width"};
			ntbAncho = new cNumericTextBox() { Width = 90, Type = eNumberType.Positive};
				sgr.AddControl(ntbAncho);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup() {	Text = "Height"};
			ntbAlto = new cNumericTextBox() { Width = 90, Type = eNumberType.Positive};
				sgr.AddControl(ntbAlto);
			grp.AddControl(sgr);
		cnt = new cContainer() {	AutoSize = eAutoSize.Height};
			chkEstirar = new cCheckBox() {	Text = "Stretch contents", ToolTip = "(Ctrl + G)"};
				cnt.AddControl(chkEstirar);
			btn = new cButton() {	Text = "Apply", Width = 90, LeftMargin = 110};
				btn.Click += AplicarTam_Click; cnt.AddControl(btn);
			grp.AddControl(cnt);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "GIF"};
		sgr = new uPropertySubgroup() {	Text = "Duration"};
			ntbDura = new cNumericTextBox() { Width = 90, Type = eNumberType.PositiveOrZeroFloating};
				sgr.AddControl(ntbDura);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	scCli.ClientArea.AddControl(spnCli); scCli.ClientArea.LayoutSuspended = false; tab.Content = scCli;
	// ** Img
	tab = tc.Tabs.Add("Image");
	scCli = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; scCli.ClientArea.SuspendLayout();
	spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup() {	Text = "DDS"};
		sgr = new uPropertySubgroup() {	Text = "Format"};
			cboDdsFmt = new cComboBox() {
					Data = new cTextValuePair<cTexture.eFormat>[] {	new cTextValuePair<cTexture.eFormat>("BC1 R5G6B5A1", cTexture.eFormat.BC1_R5G6B5A1)
						, new cTextValuePair<cTexture.eFormat>("BC5 R8G8", cTexture.eFormat.BC5_R8G8)}
					, SelectedIndex = 0};
				sgr.AddControl(cboDdsFmt);
			grp.AddControl(sgr);
		cnt = new cContainer() {	AutoSize = eAutoSize.Height};
			chkCubo = new cCheckBox() {	Text = "Cube"};
				cnt.AddControl(chkCubo);
			grp.AddControl(cnt);
		spnCli.AddControl(grp);
	scCli.ClientArea.AddControl(spnCli); scCli.ClientArea.LayoutSuspended = false; tab.Content = scCli;
	// ** Lins
	tab = tc.Tabs.Add("Lines");
	scCli = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; scCli.ClientArea.SuspendLayout();
	spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup() {	Text = "Line width"};
		ntbAnchoLin = new cNumericTextBox() {	Type = eNumberType.Positive | eNumberType.Fractional};
			ntbAnchoLin.ValueChanged += ntbAnchoLin_ValueChanged; grp.AddControl(ntbAnchoLin, false);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Line style"};
		ulsTrazo = new uLineStyle();
			grp.AddControl(ulsTrazo);
		spnCli.AddControl(grp);
	scCli.ClientArea.AddControl(spnCli); scCli.ClientArea.LayoutSuspended = false; tab.Content = scCli;
	// ** Formas
	tab = tc.Tabs.Add("Shapes");
	scCli = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; scCli.ClientArea.SuspendLayout();
	spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup() {	Text = "Rounded rectangle"};
		uptRrEsquina = new uPoint() {	Text = "Corner"};
			grp.AddControl(uptRrEsquina);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Arc"};
		uslArcAng = new uSlider() {	Text = "Angle", IsPercent = true};
			grp.AddControl(uslArcAng);
		uptArcRadio = new uPoint() {	Text = "Radius"};
			grp.AddControl(uptArcRadio);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Coordinates"};
		chkRedondear = new cCheckBox() {	Text = "Round coordinates", LeftMargin = 110};
			grp.AddControl(chkRedondear);
		chkGrid = new cCheckBox() {	Text = "Show grid", ToolTip = "(Ctrl+G)", LeftMargin = 110};
			chkGrid.CheckStateChanged += chkGrid_CheckStateChanged; grp.AddControl(chkGrid);
		chkAntialias = new cCheckBox() {	Text = "Smooth curves", LeftMargin = 110};
			grp.AddControl(chkAntialias);
		chkSnap = new cCheckBox() {	Text = "Snap lines", LeftMargin = 110};
			grp.AddControl(chkSnap);
		spnCli.AddControl(grp);
	scCli.ClientArea.AddControl(spnCli); scCli.ClientArea.LayoutSuspended = false; tab.Content = scCli;
	tc.LayoutSuspended = false; tc.Header.LayoutSuspended = false;
	AddControl(tc);
}
public void Ini()
{	ntbAnchoLin.FloatValue = Settings.Default.AnchoLin; mMod.Herrs.RefrescaTam();
	ulsTrazo.SuspendLayout(); ulsTrazo.Load(Settings.Default.LsTrazo); ulsTrazo.LayoutSuspended = false;
	uptRrEsquina.Value = Settings.Default.RrEsquina;
	uslArcAng.Value = Settings.Default.ArcAng;
		uptArcRadio.Value = Settings.Default.ArcRadio;
	chkRedondear.Checked = Settings.Default.Redondear;
	chkGrid.Checked = Settings.Default.Grid;
	chkAntialias.Checked = Settings.Default.Antialias;
	chkSnap.Checked = Settings.Default.SnapLin;
}
public void GrabaCfg()
{	Settings.Default.AnchoLin = ntbAnchoLin.FloatValue;
	Settings.Default.LsTrazo = ulsTrazo.Save();
	Settings.Default.RrEsquina = uptRrEsquina.Value;
	Settings.Default.ArcAng = uslArcAng.Value;
		Settings.Default.ArcRadio = uptArcRadio.Value;
	Settings.Default.Tam = m_ptTam;
	Settings.Default.Redondear = chkRedondear.Checked;
	Settings.Default.Grid = chkGrid.Checked;
	Settings.Default.Antialias = chkAntialias.Checked;
	Settings.Default.SnapLin = chkSnap.Checked;
}
// ** Props
public float AnchoLin
{	get														{	return (ntbAnchoLin.FloatValue > 0 ? ntbAnchoLin.FloatValue : 1);}
	set														{	wMain.TerminaEdición(); ntbAnchoLin.FloatValue = value;}
}
public cLineStyle Trazo										{	get	{	return ulsTrazo.Value;}}
public Point RrEsquina										{	get	{	return uptRrEsquina.Value;}}
public float ArcAng											{	get	{	return uslArcAng.Value;}}
public Point ArcRadio										{	get	{	return uptArcRadio.Value;}}
public bool Redondear										{	get	{	return chkRedondear.Checked;}}
public bool Grid
{	get														{	return chkGrid.Checked;}
	set														{	chkGrid.Checked = value; wMain.VistaActual?.Invalida();}
}
public bool Antialias										{	get	{	return chkAntialias.Checked;}}
public bool SnapLin											{	get	{	return chkSnap.Checked;}}
public cTexture.eFormat Format
{	get														{	return ((cTextValuePair<cTexture.eFormat>)cboDdsFmt.SelectedItem).Value;}
	set														{	cboDdsFmt.SelectedItem = new cTextValuePair<cTexture.eFormat>(null, value);}
}
public bool EsCubo
{	get														{	return chkCubo.Checked;}
	set														{	chkCubo.Checked = value;}
}
// ** Mets
public void OnVistaEnfocada(fVista visVista)
{	m_SetTam(visVista.Tam); ntbDura.FloatValue = visVista.Dura;
}
private void ntbAnchoLin_ValueChanged(object sender)		{	wMain.TerminaEdición(); mMod.Herrs.RefrescaTam();}
private void chkGrid_CheckStateChanged(object sender)		{	wMain.VistaActual?.Invalida();}
private void AplicarTam_Click(object sender)
{	Point ptTam;

		if (wMain.VistaActual == null)	return;
	ptTam = (Point)new Point(ntbAncho.IntValue, ntbAlto.IntValue).GetTruncated();
		if (ptTam.X == 0 || ptTam.X > 100000 || ptTam.Y == 0 || ptTam.Y > 100000) // Tam incorrecto: salir
		{	mDialog.MsgBoxExclamation("Invalid size", "Resize");
			return;
		}
	ptTam.Truncate(); wMain.VistaActual.Tam = ptTam;
	if (chkEstirar.Checked)	mMod.Efectos.Estira(m_ptTam, ptTam);	else	mMod.Efectos.Redim();
	m_SetTam(ptTam); Settings.Default.Tam = m_ptTam;
}
private void m_SetTam(Point value)
{	if (value != m_ptTam)	{	m_ptTam = value; ntbAncho.IntValue = (int)value.X; ntbAlto.IntValue = (int)value.Y;}
}
}
}