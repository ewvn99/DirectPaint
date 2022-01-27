using CultInf = System.Globalization.CultureInfo;
using Wew.Control;
using Wew.Media;
using Wew;

//¿edición de bmp pegado
namespace DirectPaint
{
partial class wMain : cWindow
{	string m_sRuta; bool m_bCambiado;
	readonly cCommandState m_csZoom;
	readonly cLabel lblPos, lblSelRect, lblSelTam, lblInfo;
// ** Estat
	static float s_fEscalaUsr = 1; static Point s_ptEscalaTot; static Matrix3x2 s_matEscala = Matrix3x2.Identity; // Zoom.  Escala con zoom y conv a pixeles.  Matriz
	public static fVista VistaActual						{	get	{	return (mMod.MainWnd.ActiveChildDock as fVista);}}
	public static Point EscalaTot							{	get	{	return s_ptEscalaTot;}}
	public static Matrix3x2 EscalaMatriz					{	get	{	return s_matEscala;}}
	public static void TerminaEdición()						{	mMod.Efectos.TerminaEdición3D(); VistaActual?.TerminaEdición();}
	public static void MuestraStatPos(string value)			{	mMod.MainWnd.lblPos.Text = value;}
	public static void MuestraStatRect(string value)		{	mMod.MainWnd.lblSelRect.Text = value;}
	public static void MuestraStatTam(string value)			{	mMod.MainWnd.lblSelTam.Text = value;}
// ** Ctor/dtor
public wMain()
{	cToolBar tb; cToolButton tbb; cStatusBar sb; string[] a_s;

	Icon = mRes.BmpPincel;
	// ** Cfg cmds
	eCommand.Cut.Icon = mRes.BmpCortar; eCommand.Copy.Icon = mRes.BmpCopiar; eCommand.Paste.Icon = mRes.BmpPegar;
	eCommand.Undo.Icon = mRes.BmpDeshacer; eCommand.Redo.Icon = mRes.BmpRehacer;
	// ** Cfg ctls
	tb = new cToolBar() {	Height = 36};
		tb.Items.Add("New", mRes.BmpNuevo, Nuevo_Click, eKey.ControlModifier | eKey.N);
		tb.Items.Add("Open", mRes.BmpAbrir, Abrir_Click, eKey.ControlModifier | eKey.O);
		tbb = tb.Items.Add("Save", mRes.BmpGrabar, Grabar_Click, eKey.ControlModifier | eKey.S);
			tbb.Type = eButtonType.Split; tbb.SuspendLayout(); tbb.LayoutSuspended = false;
			tbb.Menu = new cMenu();
			tbb.Menu.Items.Add("Save as...", null, GrabarComo_Click);
		tb.Add(eCommand.Separator, eCommand.Cut, eCommand.Copy, eCommand.Paste, eCommand.Separator
			, eCommand.Undo, eCommand.Redo, eCommand.Separator);
		tb.Items.Add("Tools", mRes.BmpHerr, Herrs_Click, eKey.ControlModifier | eKey.T);
		tb.Items.Add("Colors and font", mRes.BmpBrush, Colores_Click, eKey.ControlModifier | eKey.L);
		tb.Items.Add("Effect", mRes.BmpEfecto, Efectos_Click, eKey.ControlModifier | eKey.F);
		tb.Items.Add("Settings", mRes.BmpCfg, Cfg_Click, eKey.ControlModifier | eKey.I);
		tb.Items.Add("Frames", mRes.BmpFrames, Frames_Click);
		tb.Add(eCmds.EfNuevo, eCommand.Separator);
		tb.Items.Add(new cToolComboBox(eCommand.Zoom) {	Width = 120, Style = cComboBox.eStyle.DropDown, InputFilter = eInputType.Positive
			, Data = new string[] {	"10 %", "25 %", "50 %", "75 %", "100 %", "200 %", "400 %", "1000 %", "2000 %"}});
		Root.AddControl(tb); ToolStrips.Add(tb);
	sb = new cStatusBar();
		lblPos = sb.AddLabel("Mouse position", 150); sb.AddSeparator();
		lblSelRect = sb.AddLabel("Selection rectangle", 200); sb.AddSeparator();
		lblSelTam = sb.AddLabel("Selection size", 150); sb.AddSeparator();
		lblInfo = sb.AddLabel("Properties", 0); lblInfo.SuspendLayout(); lblInfo.RightMargin = 0; lblInfo.LayoutSuspended = false;
		Root.AddControl(sb);
	// ** Agregar cmds
	m_csZoom = new cCommandState(eCommand.Zoom, Zoom_Exec, true);
	LoadCommands(this, m_csZoom
		, new cCommandState(new cCommand(null, null, eKey.ControlModifier | eKey.G), Grid_Exec, true)
		, new cCommandState(new cCommand(null, null, eKey.ControlModifier | eKey.K), Muestra_Exec, true));
	// ** Cfg
	mMod.MainWnd = this;
	Root.LayoutSuspended = true;
	s_fEscalaUsr = Settings.Default.Escala;
	mMod.Frames = new kFrames(); mMod.Frames.Container = Root;
	mMod.Herrs = new kHerrs(); mMod.Herrs.Container = Root;
	mMod.Colores = new kColores(); mMod.Colores.Container = Root;
	mMod.Efectos = new kEfectos(); mMod.Efectos.Container = Root;
	mMod.Cfg = new kCfg(); mMod.Cfg.Container = Root;
	mMod.Herrs.Ini();
	mMod.Cfg.Ini();
	mMod.FondoTransp = new cBitmapBrush(mRes.BmpFondoTransp) {	ExtendModeX = eExtendMode.Wrap, ExtendModeY = eExtendMode.Wrap};
	mMod.GridLin = new cLineStyle(eDash.Solid, eCapStyle.Flat, eCapStyle.Flat, eCapStyle.Flat
		, 0, eLineJoin.Miter, eLineStyleTransform.OnePixel);
	if (Settings.Default.VerFmes)	mMod.Frames.Show();
	if (Settings.Default.VerHerrs)	mMod.Herrs.Show();
	if (Settings.Default.VerColores)	Colores_Click(null);
	if (Settings.Default.VerEfectos)	Efectos_Click(null);
	if (Settings.Default.VerCfg)		Cfg_Click(null);
	WindowState = Settings.Default.WindowState;
	// ** Abrir (o crear nuevo)
	a_s = System.Environment.GetCommandLineArgs(); m_bCambiado = true;
		if (a_s.Length == 2)	m_Abre(a_s[1]);	else	Nuevo_Click(null);
	Root.LayoutSuspended = false;
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
	PointI pti = visVista.Tam.GetTruncated(); lblInfo.Text = string.Format(CultInf.CurrentCulture, "Size: {0} x {1}", pti.X, pti.Y);
}
public override bool CanClose(eCloseReason reason = eCloseReason.User)
{	TerminaEdición();
		if (!Changed) return true;											// Ya está grabado: salir
	// ** Pedir confirmación
	switch (mDialog.MsgBoxQuestionCancel("Save changes?", "Save"))
	{	case false:	return true;											// No grabar: salir
		case null:	return false;											// No grabar y cancelar operación sgte: salir
	}
	// ** Grabar
	return m_bGraba();
}
protected override void OnChanged()
{	if (Changed != m_bCambiado)
	{	m_bCambiado = Changed;
		Text = string.Format(CultInf.CurrentCulture, "DirectPaint - {0}{1}", (m_sRuta ?? "<new>"), (m_bCambiado ? "*" : null));
	}
}
private void Nuevo_Click(object sender)
{	if (CanClose()) {	TerminaEdición(); mMod.Efectos.LimpiaEfectos(); m_SetRuta(null); mMod.Frames.Nuevo();}
}
private void Abrir_Click(object sender)
{	string s;

	if (!CanClose()) return;												// No se pudo cerrar: salir
	s = mDialog.ShowOpenFile(mMod.FILTRO_ABRIR, mMod.DLG_IMG_GUID);	if (s == null) return;
	m_Abre(s);
}
private void Grabar_Click(object sender)					{	m_bGraba();}
private void GrabarComo_Click(object sender)				{	m_bGraba(true);}
///imprimir
private void Frames_Click(object sender)					{	mMod.Frames.Show();}
private void Herrs_Click(object sender)						{	mMod.Herrs.Show();}
private void Colores_Click(object sender)
{	mMod.Colores.Show((mMod.Efectos.Visible ? (cDockControl)mMod.Efectos : mMod.Cfg));
}
private void Efectos_Click(object sender)
{	mMod.Efectos.Show((mMod.Colores.Visible ? (cDockControl)mMod.Colores : mMod.Cfg));
}
private void Cfg_Click(object sender)
{	mMod.Cfg.Show((mMod.Colores.Visible ? (cDockControl)mMod.Colores : mMod.Efectos));
}
private void Muestra_Exec(cCommandState command, object args = null)	{	mMod.Herrs.Herr = eHerr.Muestra;}
private void Grid_Exec(cCommandState command, object args = null)	{	mMod.Cfg.Grid = !mMod.Cfg.Grid;}
private void Zoom_Exec(cCommandState command, object args = null)
{	float f;

	if (float.TryParse(((string)args).Replace('%', ' '), System.Globalization.NumberStyles.AllowTrailingWhite, CultInf.CurrentCulture, out f)
			&& VistaActual != null)
		m_Zoom(VistaActual, (f / 100));
}
private void m_Abre(string sRuta)
{	TerminaEdición(); mMod.Efectos.LimpiaEfectos();
	if (mMod.Frames.Abre(sRuta)) m_SetRuta(sRuta);
}
private bool m_bGraba(bool bGuardarComo = false)
{	string sRuta;

	// ** Tomar ruta
	sRuta = (!bGuardarComo ? m_sRuta : null);
	switch ((System.IO.Path.GetExtension(sRuta) ?? "").ToUpperInvariant())
	{	case ".PNG": case ".JPG": case ".GIF": case ".TIF": case ".TIFF": case ".BMP": case ".DDS":
			break;
		default:															// ** No codificable (o nuevo): tomar otro fmt
			sRuta = mDialog.ShowSaveFile(mMod.FILTRO_GRABAR, mMod.DLG_IMG_GUID);	if (sRuta == null) return false; // Cancelado: salir
			m_sRuta = sRuta; Changed = true;
			break;
	}
	// ** Grabar
	TerminaEdición();
	if (!mMod.Frames.Graba(m_sRuta))	return false;
	Changed = false;
	return true;
}
private void m_SetRuta(string sRuta)						{	m_sRuta = sRuta; m_bCambiado = true; Changed = false;}
private void m_Zoom(fVista visVista, float fEscala)
{	float fLadoMenor, fLadoMayor;
	
	fLadoMenor = visVista.Tam.X; fLadoMayor = visVista.Tam.Y; mMath.Sort(ref fLadoMenor, ref fLadoMayor); // ** Limitar escala si el tam final no es aceptable (muy pequeño o muy grande)
	if (fEscala < s_fEscalaUsr)												// ** Reduciendo
	{	if (fEscala < 1 && fLadoMayor * fEscala < 200)	{	fEscala = 200 / fLadoMayor;	if (fEscala > 1)	fEscala = 1;} // < 100 % y queda muy pequeño: limitar
	} else if (fEscala > s_fEscalaUsr)										// ** Agrandando
	{	if (fEscala > 1 && fLadoMenor * fEscala > 2000)	{	fEscala = 2000 / fLadoMenor;	if (fEscala < 1)	fEscala = 1;} // > 100 % y queda muy grande: limitar
	}
	s_fEscalaUsr = fEscala; s_ptEscalaTot = cGraphics.PixelToDip * fEscala; s_matEscala = Matrix3x2.FromScale(s_ptEscalaTot);
	m_csZoom.SetArgs(string.Format(CultInf.CurrentCulture, "{0} %", (int)(s_fEscalaUsr * 100)));
	visVista.AplicaEscala(s_ptEscalaTot);
}
//private void m_CreaEspectro()
//{	System.IntPtr p;

//	using (cBitmap bmp = new cBitmap(401, 401, ePixelFormat._24bppBGR))
//	{	using (cBitmap.cBitmapLock bl = bmp.Lock())
//		{	for (int y = 0; y < bmp.Size.Y; y++)
//			{	p = bl.Pointer;
//				for (int x = 0; x < bmp.Size.X; x++)
//				{	ColorF.FromHSL(x / 400.0f, y / 400.0f, 0.5f).WriteBits(ref p, bmp.PixelFormat);
//				}
//				bl.Pointer += bl.Stride;
//			}
//		}
//		bmp.Save("Espectro.png");
//	}
//}
}
}