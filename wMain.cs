using System.IO;
using System.Text;
using Wew;
using Wew.Control;
using Wew.Media;
using CultInf = System.Globalization.CultureInfo;

//¿edición de bmp pegado
namespace DirectPaint;

sealed class wMain : wDocumentWindow
{	readonly cCommandState m_csZoom;
	readonly cLabel lblPos, lblSelRect, lblSelTam, lblInfo;
// ** Estat
	static float s_fEscalaUsr = 1; static Point s_ptEscalaTot; static Matrix3x2 s_matEscala = Matrix3x2.Identity; // Zoom.  Escala con zoom y conv a pixeles.  Matriz
	public static fVista VistaActual						=>	(mMod.MainWnd.ActiveForm as fVista)!;
	public static Point EscalaTot							=>	s_ptEscalaTot;
	public static Matrix3x2 EscalaMatriz					=>	s_matEscala;
	public static void TerminaEdición()						{	mMod.Efectos.TerminaEdición3D(); VistaActual?.TerminaEdición();}
	public static void MuestraStatPos(string? value)		{	mMod.MainWnd.lblPos.Text = value;}
	public static void MuestraStatRect(string? value)		{	mMod.MainWnd.lblSelRect.Text = value;}
	public static void MuestraStatTam(string? value)		{	mMod.MainWnd.lblSelTam.Text = value;}
// ** Ctor/dtor
public wMain()
{	cToolBar tb; cToolButton tbb; cStatusBar sb;

	Icon = mRes.BmpPincel;
	// ** Cfg ctls
	tb = new cToolBar {	Height = 36};
		_ = tb.Items.Add("New", eBitmap.BmpNew, Nuevo_Click, eKey.ControlModifier | eKey.N);
		_ = tb.Items.Add("Open", eBitmap.BmpOpen, Abrir_Click, eKey.ControlModifier | eKey.O);
		tbb = tb.Items.Add("Save", eBitmap.BmpSave, Grabar_Click, eKey.ControlModifier | eKey.S);
			tbb.Type = eButtonType.Split;
			tbb.Menu = new cMenu();
			_ = tbb.Menu.Items.Add("Save as...", null, GrabarComo_Click);
		tb.Add(eCommand.Separator, eCommand.Cut, eCommand.Copy, eCommand.Paste, eCommand.Separator
			, eCommand.Undo, eCommand.Redo, eCommand.Separator);
		_ = tb.Items.Add("Tools", mRes.BmpHerr, Herrs_Click, eKey.ControlModifier | eKey.T);
		_ = tb.Items.Add("Colors and font", mRes.BmpBrush, Colores_Click, eKey.ControlModifier | eKey.L);
		_ = tb.Items.Add("Effects", mRes.BmpEfecto, Efectos_Click, eKey.ControlModifier | eKey.F);
		_ = tb.Items.Add("Settings", mRes.BmpCfg, Cfg_Click, eKey.ControlModifier | eKey.I);
		_ = tb.Items.Add("Frames", mRes.BmpFrames, Frames_Click);
		tb.Add(eCmds.EfNuevo, eCommand.Separator);
		cToolComboBox tcb = new (eCommand.Zoom) {	Width = 70, Style = cComboBox.eStyle.DropDown
				, InputFilter = eInputType.Positive, MultiExecution = true};
			tcb.Load(new string[] {	"10 %", "25 %", "50 %", "75 %", "100 %", "200 %", "400 %", "1000 %", "2000 %"});
			tb.Items.Add(tcb);
		_ = tb.Items.Add("Print", mRes.BmpPrint, Print_Click);
		Root.AddControl(tb); ToolStrips.Add(tb);
	sb = new cStatusBar();
		_ = sb.AddIcon("Selection", 32, mRes.BmpSel);
			lblSelRect = sb.AddLabel("Selection rectangle", 200); sb.AddSeparator();
			lblSelTam = sb.AddLabel("Selection size", 150); sb.AddSeparator();
		_ = sb.AddIcon("Mouse position", 32, mRes.BmpCur);
			lblPos = sb.AddLabel(null, 150); sb.AddSeparator();
		_ = sb.AddIcon("Size", 32, mRes.BmpBorde);
			lblInfo = sb.AddLabel(null, 0); lblInfo.RightMargin = 0;
		Root.AddControl(sb);
	// ** Agregar cmds
	m_csZoom = new cCommandState(eCommand.Zoom, Zoom_Exec, true);
	LoadCommands(this, m_csZoom
		, new cCommandState(new cCommand(null, null, eKey.ControlModifier | eKey.G), Grid_Exec, true)
		, new cCommandState(new cCommand(null, null, eKey.ControlModifier | eKey.K), Muestra_Exec, true));
	// ** Cfg
	mMod.MainWnd = this;
	s_fEscalaUsr = Settings.Default.Escala;
	mMod.Frames = new kFrames {	Container = Root};
	mMod.Herrs = new kHerrs {	Container = Root};
	mMod.Colores = new kColores {	Container = Root};
	mMod.Efectos = new kEfectos {	Container = Root};
	mMod.Cfg = new kCfg {	Container = Root};
	mMod.Herrs.Ini();
	mMod.Cfg.Ini();
	mMod.FondoTransp = new cBitmapBrush(mRes.BmpFondoTransp, false) {	ExtendModeX = eExtendMode.Wrap, ExtendModeY = eExtendMode.Wrap};
	mMod.GridLin = new cLineStyle(eDash.Solid, eCapStyle.Flat, eCapStyle.Flat, eCapStyle.Flat
		, 0, eLineJoin.Miter, eLineStyleTransform.OnePixel);
	if (Settings.Default.VerFmes)		mMod.Frames.Show();
	if (Settings.Default.VerHerrs)		mMod.Herrs.Show();
	if (Settings.Default.VerColores)	Colores_Click(null);
	if (Settings.Default.VerEfectos)	Efectos_Click(null);
	if (Settings.Default.VerCfg)		Cfg_Click(null);
	WindowState = Settings.Default.WindowState;
	// ** Abrir (o crear nuevo)
	r_FileOpenFilter = mMod.FILTRO_ABRIR; r_FileOpenGuid = mMod.DLG_IMG_GUID;
	r_FileSaveFilter = mImaging.GetFileDialogFilter(true); r_FileSaveGuid = mMod.DLG_IMG_GUID;
	ProcessCommandLine();
}
protected override void OnClosed(eCloseReason reason)
{	mMod.Herrs.GrabaCfg();
	mMod.Colores.GrabaCfg();
	mMod.Cfg.GrabaCfg();
	Settings.Default.VerFmes = mMod.Frames.Visible; Settings.Default.VerHerrs = mMod.Herrs.Visible;
	Settings.Default.VerColores = mMod.Colores.Visible; Settings.Default.VerEfectos = mMod.Efectos.Visible;
	Settings.Default.VerCfg = mMod.Cfg.Visible;
	Settings.Default.Escala = s_fEscalaUsr; Settings.Default.WindowState = WindowState;
	Settings.Default.Save();										// Guardar cfg
	base.OnClosed(reason);
}
// ** Mets
public void Zoom(fVista visVista, bool bExpand)				{	m_Zoom(visVista, (bExpand ? s_fEscalaUsr * 2 : s_fEscalaUsr / 2));}
public void OnVistaRedim(fVista visVista)
{	m_Zoom(visVista, s_fEscalaUsr);
	PointI pti = (PointI)visVista.Tam; lblInfo.Text = $"{pti.X} x {pti.Y}";
}
public override bool CanClose(eCloseReason reason = eCloseReason.User)	{	TerminaEdición(); return base.CanClose(reason);}
protected override void OnNew()								{	mMod.Efectos.LimpiaEfectos(); mMod.Frames.NuevoCnt();}
protected override bool OnOpen(string FileName)
{	TerminaEdición(); mMod.Efectos.LimpiaEfectos();
	return mMod.Frames.AbreCnt(FileName);
}
protected override bool OnSave()							{	TerminaEdición(); return mMod.Frames.GrabaCnt(r_FileName!);}
private void Nuevo_Click(object? sender)					{	r_New();}
private void Abrir_Click(object sender)						{	r_Open();}
private void Grabar_Click(object sender)					{	_ = r_Save(mMod.Cfg.ContainerFormat == default || !mImaging.GetContainerFormat(mMod.Cfg.ContainerFormat).HasEncoder);}
private void GrabarComo_Click(object sender)				{	_ = r_Save(true);}
private void Frames_Click(object sender)					{	mMod.Frames.Show();}
private void Herrs_Click(object sender)						{	mMod.Herrs.Show();}
private void Colores_Click(object? sender)
{	mMod.Colores.Show((mMod.Efectos.Visible ? mMod.Efectos : mMod.Cfg));
}
private void Efectos_Click(object? sender)
{	mMod.Efectos.Show((mMod.Colores.Visible ? mMod.Colores : mMod.Cfg));
}
private void Cfg_Click(object? sender)
{	mMod.Cfg.Show((mMod.Colores.Visible ? mMod.Colores : mMod.Efectos));
}
private void Print_Click(object sender)
{	mMod.Print ??= new fVista.fPrint {	Container = Root};
	mMod.Print.Muestra();
}
private void Muestra_Exec(cCommand command, object? args = null)	{	mMod.Herrs.Herr = eHerr.Muestra;}
private void Grid_Exec(cCommand command, object? args = null)	{	mMod.Cfg.Grid = !mMod.Cfg.Grid;}
private void Zoom_Exec(cCommand command, object? args = null)
{	float f;

	if (float.TryParse(((string?)args)?.Replace('%', ' '), System.Globalization.NumberStyles.AllowTrailingWhite, CultInf.CurrentCulture, out f)
			&& VistaActual is not null)
		m_Zoom(VistaActual, (f / 100));
}
private void m_Zoom(fVista visVista, float fEscala)
{	float fLadoMenor, fLadoMayor;
	
	(fLadoMenor, fLadoMayor) = mMath.Sort(visVista.Tam.X, visVista.Tam.Y); // ** Limitar escala si el tam final no es aceptable (muy pequeño o muy grande)
	if (fEscala < s_fEscalaUsr)												// ** Reduciendo
	{	if (fEscala < 1 && fLadoMayor * fEscala < 200)	{	fEscala = 200 / fLadoMayor;	if (fEscala > 1)	fEscala = 1;} // < 100 % y queda muy pequeño: limitar
	} else if (fEscala > s_fEscalaUsr)										// ** Agrandando
	{	if (fEscala > 1 && fLadoMenor * fEscala > 2000)	{	fEscala = 2000 / fLadoMenor;	if (fEscala < 1)	fEscala = 1;} // > 100 % y queda muy grande: limitar
	}
	s_fEscalaUsr = fEscala; s_ptEscalaTot = cGraphics.PixelToDip * fEscala; s_matEscala = Matrix3x2.FromScale(s_ptEscalaTot);
	m_csZoom.Arguments = $"{(int)(s_fEscalaUsr * 100)} %";
	visVista.AplicaEscala(s_ptEscalaTot);
}
//private void m_CreaEspectro()
//{	System.IntPtr p; using cBitmap bmp = new (401, 401, ePixelFormat._24bppBGR); using cBitmap.cBitmapLock bl = bmp.Lock();
//
//	for (int y = 0; y < bmp.Size.Y; y++)
//	{	p = bl.Pointer;
//		for (int x = 0; x < bmp.Size.X; x++)	ColorF.FromHSL(x / 400.0f, y / 400.0f, 0.5f).WriteBits(ref p, bmp.PixelFormat);
//		bl.Pointer += bl.Stride;
//	}
//	bmp.Save("Espectro.png");
//}
}