using Wew.Control;
using Wew.Media;

namespace DirectPaint;

sealed class kCfg : cDockControl
{	Point m_ptTam;
	readonly cNumericTextBox ntbAncho, ntbAlto; readonly cCheckBox chkEstirar;
	readonly cNumericTextBox ntbDura;
	readonly cComboBox cboDdsFmt; readonly cCheckBox chkCubo;
	readonly cNumericTextBox ntbAnchoLin; readonly uLineStyle ulsTrazo;
	readonly uPoint uptRrEsquina;
	readonly uPoint uptArcRadio; readonly uSlider uslArcAng; readonly cCheckBox chkArcContrarrel, chkArcPeque;
	readonly cCheckBox chkRedondear, chkGrid, chkAntialias, chkSnap;
	public System.Guid ContainerFormat;
// ** Stat
static void OnAnchoLinChanged()						{	wMain.VistaActual.OnCfgChanged();}
// ** Ctor
public kCfg()
{	cTabControl tc; cTabControl.cTab tab; cScrollableControl scCli;
	cStackPanel spnCli; uPropertyGroup grp; uPropertySubgroup sgr; cContainer cnt; cButton btn;

	Width = 310; Text = "Settings"; IsForm = false; HideOnClose = true; Dock = eDirection.Left;
	tc = new cTabControl {	Margins = new Rect(0)};
	// ** Fme
	tab = tc.Tabs.Add("Frame");
	scCli = new cScrollableControl(); scCli.VerticalBar.Visible = true;
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Size"};
		sgr = new uPropertySubgroup {	Text = "Width"};
			ntbAncho = new cNumericTextBox { Width = 90, Type = eNumberType.Positive};
				sgr.AddControl(ntbAncho);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "Height"};
			ntbAlto = new cNumericTextBox { Width = 90, Type = eNumberType.Positive};
				sgr.AddControl(ntbAlto);
			grp.AddControl(sgr);
		cnt = new cContainer {	AutoSize = eAutoSize.Height};
			chkEstirar = new cCheckBox {	Text = "Stretch contents", ToolTip = "(Ctrl + G)"};
				cnt.AddControl(chkEstirar);
			btn = new cButton {	Text = "Apply", Width = 90, LeftMargin = 110};
				btn.Click += AplicarTam_Click; cnt.AddControl(btn);
			grp.AddControl(cnt);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "GIF"};
		sgr = new uPropertySubgroup {	Text = "Duration"};
			ntbDura = new cNumericTextBox { Width = 90, Type = eNumberType.PositiveOrZeroFloating};
				sgr.AddControl(ntbDura);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	scCli.ClientArea.AddControl(spnCli);
		tab.Content = scCli;
	// ** Img
	tab = tc.Tabs.Add("Image");
	scCli = new cScrollableControl(); scCli.VerticalBar.Visible = true;
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "DDS / TGA"};
		sgr = new uPropertySubgroup {	Text = "Format"};
			cboDdsFmt = new cComboBox();
				cboDdsFmt.Load(new cTextValuePair<ePixelFormat>[]
					{	new ("BC1 R5G6B5A1", ePixelFormat.BC1R5G6B5A1)
						, new ("BC1 Unorm SRGB", ePixelFormat.BC1UnormSrgb)
						, new ("BC2 R5G6B5A4", ePixelFormat.BC2R5G6B5A4)
						, new ("BC3 R5G6B5A8", ePixelFormat.BC3R5G6B5A8)
						, new ("BC4 R8", ePixelFormat.BC4R8)
						, new ("BC5 R8G8", ePixelFormat.BC5R8G8)
						, new ("BC6H R16G16B16", ePixelFormat.BC6HR16G16B16)
						, new ("BC7 R7G7B7A8", ePixelFormat.BC7R7G7B7A8)
						, new ("32bit BGRA", ePixelFormat.Bits32BGRA)
						, new ("32bit BGR", ePixelFormat.Bits32BGR)
						, new ("32bit RGBA", ePixelFormat.Bits32RGBA)
						, new ("32bit RGBA Unorm SRGB", ePixelFormat.Bits32RGBAUnormSrgb)
						, new ("64bit RGBA Float", ePixelFormat.Bits64RGBAHalf)
					});
				cboDdsFmt.SelectedIndex = 0;
				sgr.AddControl(cboDdsFmt);
			grp.AddControl(sgr);
		cnt = new cContainer {	AutoSize = eAutoSize.Height};
			chkCubo = new cCheckBox {	Text = "Cube"};
				cnt.AddControl(chkCubo);
			grp.AddControl(cnt);
		spnCli.AddControl(grp);
	scCli.ClientArea.AddControl(spnCli);
		tab.Content = scCli;
	// ** Lins
	tab = tc.Tabs.Add("Lines");
	scCli = new cScrollableControl(); scCli.VerticalBar.Visible = true;
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Line width"};
		ntbAnchoLin = new cNumericTextBox {	Type = eNumberType.Positive | eNumberType.Fractional};
			ntbAnchoLin.ValueChanged += ntbAnchoLin_ValueChanged; grp.AddControl(ntbAnchoLin, false);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Line style"};
		ulsTrazo = new uLineStyle();
			ulsTrazo.Changed += ulsTrazo_Changed;
			grp.AddControl(ulsTrazo);
		spnCli.AddControl(grp);
	scCli.ClientArea.AddControl(spnCli);
		tab.Content = scCli;
	// ** Formas
	tab = tc.Tabs.Add("Shapes");
	scCli = new cScrollableControl(); scCli.VerticalBar.Visible = true;
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Rounded rectangle"};
		uptRrEsquina = new uPoint {	Text = "Corner"};
			uptRrEsquina.ValueChanged += uptRrEsquina_ValueChanged;
			grp.AddControl(uptRrEsquina);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Arc"};
		uslArcAng = new uSlider {	Text = "Angle", Minimum = -90, Maximum = 90};
			uslArcAng.ValueChanged += uslArcAng_ValueChanged;
			grp.AddControl(uslArcAng);
		uptArcRadio = new uPoint {	Text = "Radius"};
			uptArcRadio.ValueChanged += uptArcRadio_ValueChanged;
			grp.AddControl(uptArcRadio);
		chkArcContrarrel = new cCheckBox {	Text = "Counter clockwise", LeftMargin = 110};
			chkArcContrarrel.CheckStateChanged += chkArcContrarrel_CheckStateChanged;
			grp.AddControl(chkArcContrarrel);
		chkArcPeque = new cCheckBox {	Text = "Small arc", LeftMargin = 110};
			chkArcPeque.CheckStateChanged += chkArcPeque_CheckStateChanged;
			grp.AddControl(chkArcPeque);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Coordinates"};
		chkRedondear = new cCheckBox {	Text = "Round coordinates", LeftMargin = 110};
			grp.AddControl(chkRedondear);
		chkGrid = new cCheckBox {	Text = "Show grid", ToolTip = "(Ctrl+G)", LeftMargin = 110};
			chkGrid.CheckStateChanged += chkGrid_CheckStateChanged;
			grp.AddControl(chkGrid);
		chkAntialias = new cCheckBox {	Text = "Smooth curves", LeftMargin = 110};
			chkAntialias.CheckStateChanged += chkAntialias_CheckStateChanged;
			grp.AddControl(chkAntialias);
		chkSnap = new cCheckBox {	Text = "Snap lines", LeftMargin = 110};
			chkSnap.CheckStateChanged += chkSnap_CheckStateChanged;
			grp.AddControl(chkSnap);
		spnCli.AddControl(grp);
	scCli.ClientArea.AddControl(spnCli);
		tab.Content = scCli;
	AddControl(tc);
}
public void Ini()
{	ntbAnchoLin.FloatValue = Settings.Default.AnchoLin; mMod.Herrs.RefrescaAnchoLin();
	ulsTrazo.Load(Settings.Default.LsTrazo);
	uptRrEsquina.Value = Settings.Default.RrEsquina;
	uslArcAng.Value = Settings.Default.ArcAng;
		uptArcRadio.Value = Settings.Default.ArcRadio;
	chkArcContrarrel.Checked = Settings.Default.ArcContrarrel;
	chkArcPeque.Checked = Settings.Default.ArcPeque;
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
	Settings.Default.ArcContrarrel = chkArcContrarrel.Checked;
	Settings.Default.ArcPeque = chkArcPeque.Checked;
	Settings.Default.Tam = m_ptTam;
	Settings.Default.Redondear = chkRedondear.Checked;
	Settings.Default.Grid = chkGrid.Checked;
	Settings.Default.Antialias = chkAntialias.Checked;
	Settings.Default.SnapLin = chkSnap.Checked;
}
// ** Props
public float AnchoLin
{	get												=>	MathF.Max(ntbAnchoLin.FloatValue, 1);
	set												{	ntbAnchoLin.FloatValue = value; OnAnchoLinChanged();}
}
public cLineStyle Trazo								=>	ulsTrazo.Value;
public Point RrEsquina								=>	uptRrEsquina.Value;
public float ArcAng									=>	uslArcAng.Value;
public Point ArcRadio								=>	uptArcRadio.Value;
public bool ArcContrarrel							=>	chkArcContrarrel.Checked;
public bool ArcPeque								=>	chkArcPeque.Checked;
public bool Redondear								=>	chkRedondear.Checked;
public bool Grid
{	get												=>	chkGrid.Checked;
	set												{	chkGrid.Checked = value; wMain.VistaActual.Invalida();}
}
public bool Antialias								=>	chkAntialias.Checked;
public bool SnapLin									=>	chkSnap.Checked;
public ePixelFormat Format
{	get														=>	((cTextValuePair<ePixelFormat>)cboDdsFmt.SelectedValue!).Value;
	set														=>	cboDdsFmt.SelectedValue = new cTextValuePair<ePixelFormat>(null, value);
}
public bool EsCubo
{	get														=>	chkCubo.Checked;
	set														=>	chkCubo.Checked = value;
}
// ** Mets
public void OnVistaEnfocada(fVista visVista)		{	m_SetTam(visVista.Tam); ntbDura.FloatValue = visVista.Dura;}
void AplicarTam_Click(object sender)
{	Point ptTam;

		if (wMain.VistaActual is null)	return;
	ptTam = new Point(ntbAncho.IntValue, ntbAlto.IntValue);
		if (ptTam.X == 0 || ptTam.X > 100000 || ptTam.Y == 0 || ptTam.Y > 100000) // Tam incorrecto: salir
		{	mDialog.MsgBoxExclamation("Invalid size", "Resize");
			return;
		}
	ptTam.Truncate(); wMain.VistaActual.Tam = ptTam;
	if (chkEstirar.Checked)	mMod.Efectos.Estira(m_ptTam, ptTam);	else	mMod.Efectos.Redim();
	m_SetTam(ptTam); Settings.Default.Tam = m_ptTam;
}
void ntbAnchoLin_ValueChanged(object sender)		{	mMod.Herrs.RefrescaAnchoLin(); OnAnchoLinChanged();}
void ulsTrazo_Changed(object sender)				{	OnAnchoLinChanged();}
void uptRrEsquina_ValueChanged(object sender)		{	wMain.VistaActual.OnCfgChanged();}
void uslArcAng_ValueChanged(object sender)			{	wMain.VistaActual.OnCfgChanged();}
void uptArcRadio_ValueChanged(object sender)		{	wMain.VistaActual.OnCfgChanged();}
void chkArcContrarrel_CheckStateChanged(object sender)	{	wMain.VistaActual.OnCfgChanged();}
void chkArcPeque_CheckStateChanged(object sender)	{	wMain.VistaActual.OnCfgChanged();}
void chkGrid_CheckStateChanged(object sender)		{	wMain.VistaActual.Invalida();}
void chkAntialias_CheckStateChanged(object sender)	{	wMain.VistaActual.OnCfgChanged();}
void chkSnap_CheckStateChanged(object sender)		{	wMain.VistaActual.OnCfgChanged();}
void m_SetTam(Point value)
{	if (value != m_ptTam)	{	m_ptTam = value; ntbAncho.IntValue = (int)value.X; ntbAlto.IntValue = (int)value.Y;}
}
}