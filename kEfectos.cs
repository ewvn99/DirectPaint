using Wew.Control;
using Wew.Media;
using Wew;

namespace DirectPaint;

sealed class kEfectos : cDockControl
{// ** Tps
	sealed class cEfEstirar : cScaleEffect
	{	public Point TamIni;
	}
	sealed class cEfRot : c3DTransformEffect										// Sólo para rotar
	{	float m_fAng;
		public float Ang
		{	get												=>	m_fAng;
			set
			{	Point pt = (wMain.VistaActual?.Tam).GetValueOrDefault();
				Matrix = Matrix4x4.FromRotation(0, 0, value, new Vector(pt.X / 2, pt.Y / 2, 0));
				m_fAng = value;
			}
		}
	}
	sealed class cEfFlip : c3DTransformEffect										// Sólo para hacer flip
	{	public void Cfg(bool bHoriz, bool bVert)
		{	Matrix4x4 m44 = Matrix4x4.Identity; Point pt = (wMain.VistaActual?.Tam).GetValueOrDefault();

			if (bHoriz) {	m44.M00 = -1; m44.M30 = pt.X;}
			if (bVert)	{	m44.M11 = -1; m44.M31 = pt.Y;}
			Matrix = m44;
		}
	}
	sealed class cCbo : uPropertySubgroup
	{	readonly cComboBox cboCbo;
		string[]? ma_sItems;
		public event dEvent SelectionChanged
		{	add												=>	cboCbo.SelectionChanged += value;
			remove											=>	cboCbo.SelectionChanged -= value;
		}
		public cCbo(float fCboAncho = 0)
		{	cboCbo = new cComboBox();	if (fCboAncho != 0)	cboCbo.Width = fCboAncho;	else	cboCbo.RightMargin = 0;
				AddControl(cboCbo);
		}
		public string[]? Items
		{	get												=>	ma_sItems;
			set												{	ma_sItems = value; cboCbo.Load(value);}
		}
		public int SelectedIndex
		{	get												=>	cboCbo.SelectedIndex;
			set												=>	cboCbo.SelectedIndex = value;
		}
	}
	sealed class uEfOrig : uPropertySubgroup
	{	c3DTransformEffect? m_3deValue;
		readonly cToolButton tbtLimpiar;
		readonly cPicture picImg;
		public event dEvent? Changed;
		public uEfOrig()
		{	cToolButton tbt;

			Text = "Source";
			tbt = new cToolButton {	Bitmap = eBitmap.BmpOpen, ToolTip = "Open"};
				tbt.Click += Abrir_Click;
				AddControl(tbt);
			cStackPanel stp = new () { Direction = eDirection.Bottom, AutoSize = eAutoSize.Both};
				tbt = new cToolButton {	Bitmap = eBitmap.BmpPaste, ToolTip = "Paste"};
					tbt.Click += Pegar_Click;
					stp.AddControl(tbt);
				tbtLimpiar = new cToolButton {	Bitmap = eBitmap.BmpDelete, ToolTip = "Clear", Visible = false};
					tbtLimpiar.Click += Limpiar_Click;
					stp.AddControl(tbtLimpiar);
				AddControl(stp);
			picImg = new cPicture {	Width = 100, Height = 69};
				AddControl(picImg);
		}
		public c3DTransformEffect? Value
		{	get										=>	m_3deValue;
			set										{	m_3deValue = value; picImg.Bitmap = null;}
		}
		public bool ShowElim
		{	get										=>	tbtLimpiar.Visible;
			set										=>	tbtLimpiar.Visible = value;
		}
		public bool Valida(string sMsj, string sTit)
		{	if (m_3deValue is null)	{	mDialog.MsgBoxExclamation(sMsj, sTit); Focus(); return false;} // Sin orig: salir
			return true;
		}
		void Abrir_Click(object sender)
		{	string? s;
	
			s = mDialog.ShowOpenFile(mMod.FILTRO_ABRIR, mMod.DLG_EF_GUID);	if (s is null) return;
		try
		{	m_Carga(new cBitmap(s));
		} catch (System.Exception ex)
		{	mDialog.MsgBoxError(ex);
		}
		}
		void Pegar_Click(object sender)
		{	cBitmap? bmp;

			bmp = mClipboard.GetBitmap();	if (bmp is null)	return;
			m_Carga(bmp); 
		}
		void Limpiar_Click(object sender)			{	Value = null; Changed?.Invoke(this);}
		void m_Carga(cBitmap bmpOrig)
		{	fVista vis = wMain.VistaActual;

				if (vis is null)	return;
			m_3deValue = new c3DTransformEffect {	Input = bmpOrig};
			vis.IniConSelEf(new Rectangle(default, vis.Tam)
				, () =>
				{	m_3deValue.SetRectangle(vis.PlaceSel.Location, vis.PlaceSel.Size / bmpOrig.Size * cGraphics.DipToPixel); // ** Los effs no consideran dpi
					mMod.Efectos.EfActual?.Invalidate();
				});
			picImg.Bitmap = bmpOrig;
			Changed?.Invoke(this);
		}
	}
	sealed class uTransform : uPropertyGroup
	{	Matrix5x4 m_m54Mat;
		readonly cCheckBox chkR, chkG, chkB, chkA; readonly uMatrix umaMat;
		public event dEvent? Changed;
		public uTransform()
		{	cContainer cnt;

			cnt = new cContainer {	AutoSize = eAutoSize.Height};
				chkR = new cCheckBox {	Text = "Red", Checked = true};
					chkR.CheckStateChanged += Ctl_Changed; cnt.AddControl(chkR);
				chkG = new cCheckBox {	Text = "Green", LeftMargin = 85, Checked = true};
					chkG.CheckStateChanged += Ctl_Changed; cnt.AddControl(chkG);
				chkB = new cCheckBox {	Text = "Blue", LeftMargin = 170, Checked = true};
					chkB.CheckStateChanged += Ctl_Changed; cnt.AddControl(chkB);
				chkA = new cCheckBox {	Text = "Alpha", LeftMargin = 255};
					chkA.CheckStateChanged += Ctl_Changed; cnt.AddControl(chkA);
				AddControl(cnt);
			umaMat = new uMatrix {	Height = 155}; umaMat.Changed += Ctl_Changed;
				umaMat.Reset(sa_sCanales, 5);
				for (int j = 0; j < 5; j++)	for (int i = 0; i < 4; i++)	umaMat.SetRanges(j, i, -100, 100); // Usar %
				AddControl(umaMat);
		}
		public float[]? Red
			=>	(chkR.Checked ? new float[] {	m_m54Mat.M00, m_m54Mat.M10, m_m54Mat.M20, m_m54Mat.M30, m_m54Mat.M40} : null);
		public float[]? Green
			=>	(chkG.Checked ? new float[] { m_m54Mat.M01, m_m54Mat.M11, m_m54Mat.M21, m_m54Mat.M31, m_m54Mat.M41} : null);
		public float[]? Blue
			=>	(chkB.Checked ? new float[] { m_m54Mat.M02, m_m54Mat.M12, m_m54Mat.M22, m_m54Mat.M32, m_m54Mat.M42} : null);
		public float[]? Alpha
			=>	(chkA.Checked ? new float[] { m_m54Mat.M03, m_m54Mat.M13, m_m54Mat.M23, m_m54Mat.M33, m_m54Mat.M43} : null);
		public void Reset()
		{	umaMat.Matrix5x4 = new Matrix5x4(0, 0, 0, 0, 25, 25, 25, 25, 50, 50, 50, 50, 75, 75, 75, 75, 100, 100, 100, 100);
			m_m54Mat = umaMat.Matrix5x4 * 0.01f;							// Conv a % (puede ser nega)
		}
		private void Ctl_Changed(object sender)
		{	if (sender == umaMat)	m_m54Mat = umaMat.Matrix5x4 * 0.01f;	// Conv a % (puede ser nega)
			Changed?.Invoke(this);
		}
	}
	class uEfLuz : uPropertyGroup
	{	readonly uSlider uslZEscala, uslReflejo; 
		readonly uVector? uvPosLuz; readonly uSlider? uslShine;				// Opcionales
		readonly cColorButton? clbColor; readonly uPoint? uptUniLong; readonly cCbo? ccbInterpol; // Opcionales
		public event dEvent? Changed;
		public uEfLuz(bool bPosLuz = true, bool bShine = true, bool bEsMake3D = false)
		{	uPropertySubgroup sgr;

			AutoSize = eAutoSize.Height;
			// ** Opcio
			if (bPosLuz)
			{	uvPosLuz = new uVector {	Text = "Light position"};
					uvPosLuz.ValueChanged += Ctl_Changed; AddControl(uvPosLuz);
			}
			if (bShine)
			{	uslShine = new uSlider {	Text = "Shine focus", Maximum = 3};
					uslShine.ValueChanged += Ctl_Changed; AddControl(uslShine);
			}
			// ** Fijos
			uslZEscala = new uSlider {	Text = "Z scale", Maximum = 30};
				uslZEscala.ValueChanged += Ctl_Changed; AddControl(uslZEscala);
			uslReflejo = new uSlider {	Text = "Reflection", Maximum = 1};
				uslReflejo.ValueChanged += Ctl_Changed; AddControl(uslReflejo);
			if (!bEsMake3D)
			{	sgr = new uPropertySubgroup {	Text = "Color"};
					clbColor = new cColorButton();
						clbColor.ColorChanged += Ctl_Changed; sgr.AddControl(clbColor);
					AddControl(sgr);
				uptUniLong = new uPoint {	Text= "Unit length"};
					uptUniLong.ValueChanged += Ctl_Changed; AddControl(uptUniLong);
				ccbInterpol = new cCbo()
						{	Items = [	"Nearest neighbor", "Linear", "Cubic"
								, "MultisampleLinear", "Anisotropic", "HighQualityCubic"]
							, SelectedIndex = 0, Text = "Interpolation" 
						};
					ccbInterpol.SelectionChanged += Ctl_Changed; AddControl(ccbInterpol);
			}
		}
		public Vector PosLuz								=>	uvPosLuz!.Value;
		public float Shine									=>	uslShine!.Value;
		public virtual void IniEfecto()
		{	if (uvPosLuz is not null)	uvPosLuz.Value = default;
			if (uslShine is not null)	uslShine.Value = 0.8f;
			uslZEscala.Value = 10; uslReflejo.Value = 0.8f;
			if (clbColor is not null)
			{	clbColor.Color = eColor.White; uptUniLong!.Value = new Point(1, 1);
				ccbInterpol!.SelectedIndex = (int)eInterpolation.Linear;
			}
		}
		public void IniEfectoMake3D()						{	IniEfecto(); uslZEscala.Value = 3; uslShine!.Value = 2.5f;}
		public void AplicaEfecto(cLightEffect leEf)
		{	leEf.ZScale = uslZEscala.Value; leEf.Reflection = uslReflejo.Value;
			leEf.Color = clbColor!.Color; leEf.KernelUnitLength = uptUniLong!.Value;
			leEf.ScaleMode = (eInterpolation)ccbInterpol!.SelectedIndex;
		}
		public void AplicaEfecto(cMake3DEffect e3dEf)
		{	e3dEf.ZScale = uslZEscala.Value; e3dEf.Reflection = uslReflejo.Value;
			e3dEf.LightPosition = uvPosLuz!.Value; e3dEf.ShineFocus = uslShine!.Value;
		}
		protected void Ctl_Changed(object sender)			{	Changed?.Invoke(this);}
	}
	sealed class uLuzSpot : uEfLuz
	{	readonly uVector uvApunta; readonly uSlider uslFoco, uslCono;
		public uLuzSpot(bool bShine = true) : base(true, bShine)
		{	uvApunta = new uVector {	Text = "Points at"};
				uvApunta.ValueChanged += Ctl_Changed; InsertControl(1, uvApunta);
			uslFoco = new uSlider {	Text= "Focus", Maximum = 200};
				uslFoco.ValueChanged += Ctl_Changed; InsertControl(2, uslFoco);
			uslCono = new uSlider {	Text= "Cone angle", Maximum = 90};
				uslCono.ValueChanged += Ctl_Changed; InsertControl(3, uslCono);
		}
		public Vector Apunta								=>	uvApunta.Value;
		public float Foco									=>	uslFoco.Value;
		public float Cono									=>	uslCono.Value;
		public override void IniEfecto()
		{	Point pt = (wMain.VistaActual?.Tam).GetValueOrDefault(Point.Identity); uvApunta.Value = new Vector(pt.X / 2, pt.Y / 2, 0);
			uslFoco.Value = 10; uslCono.Value = 10; base.IniEfecto();
		}
	}
	sealed class uLuzDist : uEfLuz
	{	readonly uSlider uslAcimut, uslEleva;
		public uLuzDist(bool bShine = true) : base(false, bShine)
		{	uslAcimut = new uSlider {	Text= "Azimuth", Maximum = 360};
				uslAcimut.ValueChanged += Ctl_Changed; InsertControl(0, uslAcimut);
			uslEleva = new uSlider {	Text= "Elevation", Maximum = 360};
				uslEleva.ValueChanged += Ctl_Changed; InsertControl(1, uslEleva);
		}
		public float Acimut									=>	uslAcimut.Value;
		public float Eleva									=>	uslEleva.Value;
		public override void IniEfecto()					{	uslAcimut.Value = 0; uslEleva.Value = 0; base.IniEfecto();}
	}
	delegate cEffect dCreaEf();
// ** Cpos
	cEffect? m_efEfActual;
	cPrimObj3D? m_p3dActual;
	readonly uSlider uslHue, uslSat; readonly cCbo ccbColorPresets; readonly uMatrix umaColor, umaGammaT;
		readonly uTransform umaDiscreT, umaTableT;
		readonly uSlider uslTurbuFrec, uslTurbuOctaves; readonly cNumericTextBox ntTurbuSeed; readonly cCheckBox chkTurbuNoise, chkTurbuStitch;
	readonly uSlider uslBrilloWX, uslBrilloWY, uslBrilloBX, uslBrilloBY;
	readonly uSlider uslContrasContras;
	readonly uSlider uslSharpenSharp, uslSharpenLim;
	readonly uSlider uslEmbossAlto, uslEmbossDir;
	readonly uSlider uslEdgeStrength, uslEdgeBlur; readonly cCbo ccbEdgeMode; readonly cCheckBox chkEdgeOverlay;
	readonly cColorButton clbVigneColor; readonly uSlider uslVigneTran, uslVigneStren;
	//readonly cColorButton clbChrColor; readonly uSlider uslChrTol; readonly cCheckBox chkChrInv, chkChrFea;
	readonly uSlider uslGauss, uslDirBlurDesv, uslDirBlurAng, uslConvolveDivi, uslConvolveBias;
		readonly cCbo ccbConvolvePresets; readonly uMatrix umaConvolve; readonly uPoint uptConvolveDesp;
		readonly cCheckBox chkConvolveAlfa, chkThickThick; readonly uSlider uslThickKAncho, uslThickKAlto, uslDisplaceEscala;
		readonly uEfOrig uoDisplaceOrig; readonly cCbo ccbDisplaceX; readonly cComboBox cboDisplaceY;
		readonly cButton btnDft;
	readonly uSlider uslShadowDesv; readonly uPoint uptShadowOffset; readonly cColorButton clbShadowColor;
		readonly uEfLuz leMake3D, lePointSpec, lePointDiff; readonly uLuzSpot lesSpotSpec, lesSpotDiff;
		readonly uLuzDist ledDistSpec, ledDistDiff;
		readonly uSlider uslMake3DThickness;
	readonly uSlider uslRot, uslWaveDesp, uslWaveSkewX, uslWaveSkewY;
		readonly cCheckBox chkFlipH, chkFlipV;
		readonly cCbo ccbVertPresets; readonly uMatrix umaVert;
		readonly cCbo ccbTileModoX, ccbTileModoY;
		readonly uSlider uslRippleFrec, uslRippleFase, uslRippleAmp, uslRippleSpread, uslRippleLim;
			readonly uPoint uptRippleCenter;
		readonly uSlider uslBendAng, uslBendDist, uslBendRadio;
		readonly uSlider uslWhirlRadio, uslWhirlSpin; readonly uPoint uptWhirlCenter;
		readonly uSlider uslBubbleRadio, uslBubbleFlatten, uslBubbleFalloff, uslBubbleLuz; readonly uPoint uptBubbleCenter;
	readonly uEfOrig uoBlendOrig, uoComposiOrig, uoAritOrig; readonly cCbo ccbBlendModo, ccbComposiModo;
	readonly uSlider uslAritDestxSource, uslAritDest, uslAritSource, uslAritOffset;
	readonly uEfOrig uoColorKeyFondo; readonly cColorButton clbColorKeyFondoColor;
		readonly cColorButton clbColorKeyColor; readonly cToolButton tbbColorKeyMuestra;
		readonly uSlider uslColorKeyTolera;
	readonly u3DModelControl u3dObj;
// ** Stat
	static readonly string[] sa_sCanales = ["Red", "Green", "Blue", "Alpha"];
// ** Ctor/dtor
public kEfectos()
{	cTabControl tc; cTabControl.cTab tab; cScrollableControl scTabCli; cStackPanel spnCli;
	uPropertyGroup grp; uPropertySubgroup sgr; cContainer cnt; cToolButton tbt; int i, j;

	Width = 390; IsForm = false; Text = "Effects"; HideOnClose = true; Dock = eDirection.Left;
	tc = new cTabControl {	Margins = new Rect(0)};
	// ** Color
	tab = tc.Tabs.Add("Color");
	scTabCli = new cScrollableControl(); scTabCli.VerticalBar.Visible = true;
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Hue / Saturation"};
		uslHue = new uSlider {	Text = "Hue", IsPercent = true};
			uslHue.ValueChanged += uslHueSat_ValueChanged; grp.AddControl(uslHue);
		uslSat = new uSlider {	Text = "Saturation", IsPercent = true};
			uslSat.ValueChanged += uslHueSat_ValueChanged; grp.AddControl(uslSat);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Color matrix"};
		ccbColorPresets = new cCbo()
				{	Items = [	"Default", "Red channel", "Green channel", "Blue channel", "Yellow channel"
						, "Magenta channel", "Cyan channel", "Red", "Green", "Blue", "Yellow", "Magenta", "Cyan", "Black", "White"
						, "Gray scale", "Invert", "Swap red-green", "Swap red-blue", "Swap green-blue", "Image mask (luminance to alpha)"
						, "Invert alpha"]
					, SelectedIndex = 0, Text = "Presets"
				};
			ccbColorPresets.SelectionChanged += cboColorPresets_SelectionChanged; grp.AddControl(ccbColorPresets);
		umaColor = new uMatrix {	Height = 155};
			umaColor.Changed += umaColor_Changed; grp.AddControl(umaColor);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Gamma transfer"};
		umaGammaT = new uMatrix {	Height = 135};
			umaGammaT.Changed += umaGammaT_Changed; grp.AddControl(umaGammaT);
		spnCli.AddControl(grp);
	umaDiscreT = new uTransform {	Text = "Discrete transfer"}; umaDiscreT.Changed += DiscreT_Changed;
		spnCli.AddControl(umaDiscreT);
	umaTableT = new uTransform {	Text = "Table transfer"}; umaTableT.Changed += TableT_Changed;
		spnCli.AddControl(umaTableT);
	grp = new uPropertyGroup {	Text = "Turbulence"};
		uslTurbuFrec = new uSlider {	Text = "Base frequency", Maximum = 100};
			uslTurbuFrec.ValueChanged += Turbu_Changed; grp.AddControl(uslTurbuFrec);
		uslTurbuOctaves = new uSlider {	Text = "Octaves", Maximum = 5};
			uslTurbuOctaves.ValueChanged += Turbu_Changed; grp.AddControl(uslTurbuOctaves);
		sgr = new uPropertySubgroup {	Text = "Seed"};
			ntTurbuSeed = new cNumericTextBox {	Type = eNumberType.Zero | eNumberType.Positive, Width = 87};
				ntTurbuSeed.ValueChanged += Turbu_Changed; sgr.AddControl(ntTurbuSeed);
			grp.AddControl(sgr);
		cnt = new cContainer {	AutoSize = eAutoSize.Height};
			chkTurbuNoise = new cCheckBox {	Text = "Noise"};
				chkTurbuNoise.CheckStateChanged += Turbu_Changed; cnt.AddControl(chkTurbuNoise);
			chkTurbuStitch = new cCheckBox {	Text = "Stitchable", LeftMargin = 110};
				chkTurbuStitch.CheckStateChanged += Turbu_Changed; cnt.AddControl(chkTurbuStitch);
			grp.AddControl(cnt);
		spnCli.AddControl(grp);
	scTabCli.ClientArea.AddControl(spnCli);
		tab.Content = scTabCli;
	// ** Foto
	tab = tc.Tabs.Add("Photo");
	scTabCli = new cScrollableControl(); scTabCli.VerticalBar.Visible = true;
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Brightness"};
		uslBrilloWX = new uSlider {	Text = "White point X", IsPercent = true};
			uslBrilloWX.ValueChanged += uslBrillo_ValueChanged; grp.AddControl(uslBrilloWX);
		uslBrilloWY = new uSlider {	Text = "White point Y", IsPercent = true};
			uslBrilloWY.ValueChanged += uslBrillo_ValueChanged; grp.AddControl(uslBrilloWY);
		uslBrilloBX = new uSlider {	Text = "Black point X", IsPercent = true};
			uslBrilloBX.ValueChanged += uslBrillo_ValueChanged; grp.AddControl(uslBrilloBX);
		uslBrilloBY = new uSlider {	Text = "Black point Y", IsPercent = true};
			uslBrilloBY.ValueChanged += uslBrillo_ValueChanged; grp.AddControl(uslBrilloBY);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Contrast"};
		uslContrasContras = new uSlider {	Text = "Contrast", IsPercent = true};
			uslContrasContras.ValueChanged += Contrast_Changed; grp.AddControl(uslContrasContras);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Sharpen"};
		uslSharpenSharp = new uSlider {	Text = "Sharpness", Maximum = 10};
			uslSharpenSharp.ValueChanged += Sharpen_Changed; grp.AddControl(uslSharpenSharp);
		uslSharpenLim = new uSlider {	Text = "Threshold", IsPercent = true};
			uslSharpenLim.ValueChanged += Sharpen_Changed; grp.AddControl(uslSharpenLim);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Emboss"};
		uslEmbossAlto = new uSlider {	Text = "Height", Maximum = 20};
			uslEmbossAlto.ValueChanged += Emboss_Changed; grp.AddControl(uslEmbossAlto);
		uslEmbossDir = new uSlider {	Text = "Direction", Maximum = 360};
			uslEmbossDir.ValueChanged += Emboss_Changed; grp.AddControl(uslEmbossDir);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Edge detection"};
		uslEdgeStrength = new uSlider {	Text = "Strength", IsPercent = true};
			uslEdgeStrength.ValueChanged += EdgeDetection_Changed; grp.AddControl(uslEdgeStrength);
		uslEdgeBlur = new uSlider {	Text = "Blur radius", IsPercent = true};
			uslEdgeBlur.ValueChanged += EdgeDetection_Changed; grp.AddControl(uslEdgeBlur);
		ccbEdgeMode = new cCbo {	Text = "Mode", Items = ["Sobel", "Prewitt"]};
			ccbEdgeMode.SelectionChanged += EdgeDetection_Changed; grp.AddControl(ccbEdgeMode);
		chkEdgeOverlay = new cCheckBox {	Text = "Overlay edges", LeftMargin = 110};
			chkEdgeOverlay.CheckStateChanged += EdgeDetection_Changed; grp.AddControl(chkEdgeOverlay);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Vignette"};
		sgr = new uPropertySubgroup {	Text = "Color"};
			clbVigneColor = new cColorButton();
				clbVigneColor.ColorChanged += Vigne_Changed; sgr.AddControl(clbVigneColor);
			grp.AddControl(sgr);
		uslVigneTran = new uSlider {	Text = "Transition size", IsPercent = true};
			uslVigneTran.ValueChanged += Vigne_Changed; grp.AddControl(uslVigneTran);
		uslVigneStren = new uSlider {	Text = "Strength", IsPercent = true};
			uslVigneStren.ValueChanged += Vigne_Changed; grp.AddControl(uslVigneStren);
		spnCli.AddControl(grp);
	//grp = new uPropertyGroup {	Text = "chr"};
	//	sgr = new uPropertySubgroup {	Text = "Color"};
	//		clbChrColor = new cColorButton();
	//			clbChrColor.ColorChanged += Chr_Changed; sgr.AddControl(clbChrColor);
	//		grp.AddControl(sgr);
	//	uslChrTol = new uSlider {	Text = "tol", IsPercent = true};
	//		uslChrTol.ValueChanged += Chr_Changed; grp.AddControl(uslChrTol);
	//	chkChrInv = new cCheckBox {	Text = "inv", LeftMargin = 110};
	//		chkChrInv.CheckStateChanged += Chr_Changed; grp.AddControl(chkChrInv);
	//	chkChrFea = new cCheckBox {	Text = "fea", LeftMargin = 110};
	//		chkChrFea.CheckStateChanged += Chr_Changed; grp.AddControl(chkChrFea);
	//	spnCli.AddControl(grp);
	scTabCli.ClientArea.AddControl(spnCli);
		tab.Content = scTabCli;
	// ** Filtro
	tab = tc.Tabs.Add("Filter");
	scTabCli = new cScrollableControl(); scTabCli.VerticalBar.Visible = true;
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Gaussian blur"};
		uslGauss = new uSlider {	Text = "Deviation", IsPercent = true};
			uslGauss.ValueChanged += uslGauss_ValueChanged; grp.AddControl(uslGauss);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Directional blur"};
		uslDirBlurDesv = new uSlider {	Text = "Deviation", IsPercent = true};
			uslDirBlurDesv.ValueChanged += uslDirBlur_ValueChanged; grp.AddControl(uslDirBlurDesv);
		uslDirBlurAng = new uSlider {	Text = "Angle", IsPercent = true};
			uslDirBlurAng.ValueChanged += uslDirBlur_ValueChanged; grp.AddControl(uslDirBlurAng);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Convolve matrix"};
		ccbConvolvePresets = new cCbo {	Text = "Presets"
				, Items = ["Default", "Blur", "Sharpen", "Edge detection", "Emboss"], SelectedIndex = 0};
			ccbConvolvePresets.SelectionChanged += cboConvolvePresets_SelectionChanged; grp.AddControl(ccbConvolvePresets);
		umaConvolve = new uMatrix {	Height = 115}; umaConvolve.Changed += Convolve_Changed;
			umaConvolve.Open += ConvolveAbrir_Click; umaConvolve.Save += ConvolveGrabar_Click; grp.AddControl(umaConvolve);
		uptConvolveDesp = new uPoint {	Text = "Offset"};
			uptConvolveDesp.ValueChanged += Convolve_Changed; grp.AddControl(uptConvolveDesp);
		uslConvolveDivi = new uSlider {	Text = "Divisor", Minimum = -20, Maximum = 20, SmallChange = 0.5f};
			uslConvolveDivi.ValueChanged += Convolve_Changed; grp.AddControl(uslConvolveDivi);
		uslConvolveBias = new uSlider {	Text = "Bias", Minimum = -2, Maximum = 2, SmallChange = 0.02f};
			uslConvolveBias.ValueChanged += Convolve_Changed; grp.AddControl(uslConvolveBias);
		chkConvolveAlfa = new cCheckBox {	Text = "Preserve alpha", LeftMargin = 110};
			chkConvolveAlfa.CheckStateChanged += Convolve_Changed; grp.AddControl(chkConvolveAlfa);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Thickness"};
		chkThickThick = new cCheckBox {	Text = "Thick"};
			chkThickThick.CheckStateChanged += Thick_Changed; grp.AddControl(chkThickThick);
		uslThickKAncho = new uSlider {	Text = "Width", Minimum = 1};
			uslThickKAncho.ValueChanged += Thick_Changed; grp.AddControl(uslThickKAncho);
		uslThickKAlto = new uSlider {	Text = "Height", Minimum = 1};
			uslThickKAlto.ValueChanged += Thick_Changed; grp.AddControl(uslThickKAlto);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Displacement"};
		uoDisplaceOrig = new uEfOrig {	Text = "Map"};
			uoDisplaceOrig.Changed += Displace_Changed; grp.AddControl(uoDisplaceOrig);
		uslDisplaceEscala = new uSlider {	Text = "Scale", Maximum = 1000};
			uslDisplaceEscala.ValueChanged += Displace_Changed; grp.AddControl(uslDisplaceEscala);
		ccbDisplaceX = new cCbo(82) {	Text = "Channels", Items = ["R", "G", "B", "A"]};
			ccbDisplaceX.SelectionChanged += Displace_Changed;
			cboDisplaceY = new cComboBox {	Width = 82};
				cboDisplaceY.Load(ccbDisplaceX.Items);
				cboDisplaceY.SelectionChanged += Displace_Changed;
				ccbDisplaceX.AddControl(cboDisplaceY);
			grp.AddControl(ccbDisplaceX);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Discrete Fourier Transform"};
		btnDft = new cButton {	Text = "Apply", LeftMargin = 110};
			btnDft.Click += Dft_Changed; grp.AddControl(btnDft, false);
		spnCli.AddControl(grp);
	scTabCli.ClientArea.AddControl(spnCli);
		tab.Content = scTabCli;
	// ** Iluminación
	tab = tc.Tabs.Add("Lighting");
	scTabCli = new cScrollableControl(); scTabCli.VerticalBar.Visible = true;
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Shadow"};
		uslShadowDesv = new uSlider {	Text = "Deviation", IsPercent = true};
			uslShadowDesv.ValueChanged += Shadow_Changed; grp.AddControl(uslShadowDesv);
		uptShadowOffset = new uPoint {	Text = "Offset"};
			uptShadowOffset.ValueChanged += Shadow_Changed; grp.AddControl(uptShadowOffset);
		sgr = new uPropertySubgroup {	Text = "Color"};
			clbShadowColor = new cColorButton();
				clbShadowColor.ColorChanged += Shadow_Changed; sgr.AddControl(clbShadowColor);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	leMake3D = new uEfLuz(true, true, true) {	Text = "Relief"};
			leMake3D.Changed += Make3D_Changed;
		uslMake3DThickness = new uSlider {	Text = "Thickness", Maximum = 20};
			uslMake3DThickness.ValueChanged += Make3D_Changed; leMake3D.InsertControl(0, uslMake3DThickness);
		spnCli.AddControl(leMake3D);
	lePointSpec = new uEfLuz {	Text = "Point specular"};
		lePointSpec.Changed += PointSpec_Changed; spnCli.AddControl(lePointSpec);
	lePointDiff = new uEfLuz(true, false) {	Text = "Point diffuse"};
		lePointDiff.Changed += PointDiff_Changed; spnCli.AddControl(lePointDiff);
	lesSpotSpec = new uLuzSpot {	Text = "Spot specular"};
		lesSpotSpec.Changed += SpotSpec_Changed; spnCli.AddControl(lesSpotSpec);
	lesSpotDiff = new uLuzSpot(false) {	Text = "Spot diffuse"};
		lesSpotDiff.Changed += SpotDiff_Changed; spnCli.AddControl(lesSpotDiff);
	ledDistSpec = new uLuzDist {	Text = "Distant specular"};
		ledDistSpec.Changed += DistSpec_Changed; spnCli.AddControl(ledDistSpec);
	ledDistDiff = new uLuzDist(false) {	Text = "Distant diffuse"};
		ledDistDiff.Changed += DistDiff_Changed; spnCli.AddControl(ledDistDiff);
	scTabCli.ClientArea.AddControl(spnCli);
		tab.Content = scTabCli;
	// ** Transformación
	tab = tc.Tabs.Add("Transform");
	scTabCli = new cScrollableControl(); scTabCli.VerticalBar.Visible = true;
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Rotation"};
		uslRot = new uSlider {	Text = "Value", Maximum = 360};
			uslRot.ValueChanged += uslRot_ValueChanged; grp.AddControl(uslRot);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Flip"};
		cnt = new cContainer {	AutoSize = eAutoSize.Height};
			chkFlipH = new cCheckBox {	Text = "Horizontal"};
				chkFlipH.CheckStateChanged += Flip_CheckStateChanged; cnt.AddControl(chkFlipH);
			chkFlipV = new cCheckBox {	Text = "Vertical", LeftMargin = 150};
				chkFlipV.CheckStateChanged += Flip_CheckStateChanged; cnt.AddControl(chkFlipV);
			grp.AddControl(cnt);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Coordinate matrix"};
		ccbVertPresets = new cCbo {	Items = [	"Default", "Stretch", "Shrink", "Offset"
					, "Skew X", "Skew Y", "Yaw", "Pitch", "Flip horizontal", "Flip vertical"]
				, SelectedIndex = 0, Text = "Presets"};
			ccbVertPresets.SelectionChanged += cboVertPresets_SelectionChanged; grp.AddControl(ccbVertPresets);
		umaVert = new uMatrix {	Height = 135}; umaVert.Changed += umaVert_Changed;
			grp.AddControl(umaVert);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Tile"};
		ccbTileModoX = new cCbo {	Text = "Mode X", Items = ["Clamp", "Wrap", "Mirror"]};
			ccbTileModoX.SelectionChanged += Tile_Changed; grp.AddControl(ccbTileModoX);
		ccbTileModoY = new cCbo {	Text = "Mode Y", Items = ccbTileModoX.Items};
			ccbTileModoY.SelectionChanged += Tile_Changed; grp.AddControl(ccbTileModoY);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Wave"};
		uslWaveDesp = new uSlider {	Text = "Offset"};
			uslWaveDesp.ValueChanged += Wave_Changed; grp.AddControl(uslWaveDesp);
		uslWaveSkewX = new uSlider {	Text = "Skew X", Minimum = -5, Maximum = 5};
			uslWaveSkewX.ValueChanged += Wave_Changed; grp.AddControl(uslWaveSkewX);
		uslWaveSkewY = new uSlider {	Text = "Skew Y", Minimum = -5, Maximum = 5};
			uslWaveSkewY.ValueChanged += Wave_Changed; grp.AddControl(uslWaveSkewY);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Ripple"};
		uslRippleFrec = new uSlider {	Text = "Frequency", Maximum = 100};
			uslRippleFrec.ValueChanged += Ripple_Changed; grp.AddControl(uslRippleFrec);
		uslRippleFase = new uSlider {	Text = "Phase", Minimum = -10, Maximum = 10};
			uslRippleFase.ValueChanged += Ripple_Changed; grp.AddControl(uslRippleFase);
		uslRippleAmp = new uSlider {	Text = "Amplitude", Minimum = 0.0001f, Maximum = 1000};
			uslRippleAmp.ValueChanged += Ripple_Changed; grp.AddControl(uslRippleAmp);
		uslRippleSpread = new uSlider {	Text = "Spread", Minimum = 0.0001f, Maximum = 50};
			uslRippleSpread.ValueChanged += Ripple_Changed; grp.AddControl(uslRippleSpread);
		uptRippleCenter = new uPoint {	Text = "Center"};
			uptRippleCenter.ValueChanged += Ripple_Changed; grp.AddControl(uptRippleCenter);
		uslRippleLim = new uSlider {	Text = "Limit", Minimum = 0.0001f, Maximum = 2};
			uslRippleLim.ValueChanged += Ripple_Changed; grp.AddControl(uslRippleLim);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Bend"};
		uslBendAng = new uSlider {	Text = "Angle", Maximum = 90};
			uslBendAng.ValueChanged += Bend_Changed; grp.AddControl(uslBendAng);
		uslBendDist = new uSlider {	Text = "Distance", Minimum = -1, Maximum = 1};
			uslBendDist.ValueChanged += Bend_Changed; grp.AddControl(uslBendDist);
		uslBendRadio = new uSlider {	Text = "Radius", IsPercent = true};
			uslBendRadio.ValueChanged += Bend_Changed; grp.AddControl(uslBendRadio);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Whirl"};
		uptWhirlCenter = new uPoint {	Text = "Center"};
			uptWhirlCenter.ValueChanged += Whirl_Changed; grp.AddControl(uptWhirlCenter);
		uslWhirlRadio = new uSlider {	Text = "Radius", Minimum = 0.0001f, Maximum = 4};
			uslWhirlRadio.ValueChanged += Whirl_Changed; grp.AddControl(uslWhirlRadio);
		uslWhirlSpin = new uSlider {	Text = "Spin", Minimum = -3, Maximum = 3};
			uslWhirlSpin.ValueChanged += Whirl_Changed; grp.AddControl(uslWhirlSpin);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Bubble"};
		uptBubbleCenter = new uPoint {	Text = "Center"};
			uptBubbleCenter.ValueChanged += Bubble_Changed; grp.AddControl(uptBubbleCenter);
		uslBubbleRadio = new uSlider {	Text = "Radius", Minimum = 0.0001f, Maximum = 2};
			uslBubbleRadio.ValueChanged += Bubble_Changed; grp.AddControl(uslBubbleRadio);
		uslBubbleFlatten = new uSlider {	Text = "Flatten", IsPercent = true};
			uslBubbleFlatten.ValueChanged += Bubble_Changed; grp.AddControl(uslBubbleFlatten);
		uslBubbleFalloff = new uSlider {	Text = "Falloff", Maximum = 50};
			uslBubbleFalloff.ValueChanged += Bubble_Changed; grp.AddControl(uslBubbleFalloff);
		uslBubbleLuz = new uSlider {	Text = "Light", Minimum = -1, Maximum = 1};
			uslBubbleLuz.ValueChanged += Bubble_Changed; grp.AddControl(uslBubbleLuz);
		spnCli.AddControl(grp);
	scTabCli.ClientArea.AddControl(spnCli);
		tab.Content = scTabCli;
	// ** Composición
	tab = tc.Tabs.Add("Composition");
	scTabCli = new cScrollableControl(); scTabCli.VerticalBar.Visible = true;
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Blend"};
		uoBlendOrig = new uEfOrig();
			uoBlendOrig.Changed += Blend_Changed; grp.AddControl(uoBlendOrig);
		ccbBlendModo = new cCbo {	Text = "Mode"
				, Items = [	"Multiply", "Screen", "Darken", "Lighten", "Dissolve", "ColorBurn", "LinearBurn"
					, "DarkerColor", "LighterColor", "ColorDodge", "LinearDodge", "Overlay", "SoftLight", "HardLight", "VividLight"
					, "LinearLight", "PinLight", "HardMix", "Difference", "Exclusion", "Hue", "Saturation", "Color", "Luminosity"
					, "Subtract", "Division"]};
			ccbBlendModo.SelectionChanged += Blend_Changed; grp.AddControl(ccbBlendModo);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Composite"};
		uoComposiOrig = new uEfOrig();
			uoComposiOrig.Changed += Composi_Changed; grp.AddControl(uoComposiOrig);
		ccbComposiModo = new cCbo {	Text = "Mode"
				, Items = [	"Default", "DestinationOver", "SourceIn", "DestinationIn", "SourceOut"
					, "DestinationOut", "SourceAtop", "DestinationAtop", "Xor", "Plus", "SourceCopy", "BoundedSourceCopy", "MaskInvert"]};
			ccbComposiModo.SelectionChanged += Composi_Changed; grp.AddControl(ccbComposiModo);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Arithmetic"};
		uoAritOrig = new uEfOrig();
			uoAritOrig.Changed += Arit_Changed; grp.AddControl(uoAritOrig);
		uslAritDestxSource = new uSlider {	Text = "Source x Dest", Minimum = -100, Maximum = 100};
			uslAritDestxSource.ValueChanged += Arit_Changed; grp.AddControl(uslAritDestxSource);
		uslAritDest = new uSlider {	Text = "Destination", Minimum = -100, Maximum = 100};
			uslAritDest.ValueChanged += Arit_Changed; grp.AddControl(uslAritDest);
		uslAritSource = new uSlider {	Text = "Source", Minimum = -100, Maximum = 100};
			uslAritSource.ValueChanged += Arit_Changed; grp.AddControl(uslAritSource);
		uslAritOffset = new uSlider {	Text = "Offset", Minimum = -100, Maximum = 100};
			uslAritOffset.ValueChanged += Arit_Changed; grp.AddControl(uslAritOffset);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Color key"};
		uoColorKeyFondo = new uEfOrig {	Text = "Background", ShowElim = true};
			uoColorKeyFondo.Changed += ColorKey_Changed;
			grp.AddControl(uoColorKeyFondo);
		sgr = new uPropertySubgroup {	Text = "Background color"};
			clbColorKeyFondoColor = new cColorButton();
				clbColorKeyFondoColor.ColorChanged += ColorKey_Changed;
				sgr.AddControl(clbColorKeyFondoColor);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "Color key"};
			tbbColorKeyMuestra = new cToolButton {	Bitmap = mRes.BmpMuestra, ToolTip = "Pick color"};
				tbbColorKeyMuestra.Click += tbbColorKeyMuestra_Click; tbbColorKeyMuestra.MouseDown += tbbColorKeyMuestra_MouseDown;
				 sgr.AddControl(tbbColorKeyMuestra);
			clbColorKeyColor = new cColorButton();
				clbColorKeyColor.ColorChanged += ColorKey_Changed;
				sgr.AddControl(clbColorKeyColor);
			grp.AddControl(sgr);
		uslColorKeyTolera = new uSlider {	Text = "Tolerance", Value = 50};
			uslColorKeyTolera.ValueChanged += ColorKey_Changed;
			grp.AddControl(uslColorKeyTolera);
		spnCli.AddControl(grp);
	scTabCli.ClientArea.AddControl(spnCli);
		tab.Content = scTabCli;
	// ** Obj 3D
	tab = tc.Tabs.Add("3D object");
	scTabCli = new cScrollableControl(); scTabCli.VerticalBar.Visible = true;
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Model"};
		sgr = new uPropertySubgroup {	Text = "Add"};
			tbt = new cToolButton {	Bitmap = eBitmap.BmpOpen, ToolTip = "Open"};
				tbt.Click += tbt3DObjAbrir_Click; sgr.AddControl(tbt);
			tbt = new cToolButton {	Bitmap = eBitmap.BmpPaste, ToolTip = "Paste"};
				tbt.Click += tbt3DObjPegar_Click; sgr.AddControl(tbt);
			grp.AddControl(sgr);
		u3dObj = new u3DModelControl {	ApplyMeshToTree = true}; u3dObj.ModelChanged += u3dObj_Changed;
			u3dObj.ModelInvalidated += u3dObj_ModelInvalidated; grp.AddControl(u3dObj);
		spnCli.AddControl(grp);
	scTabCli.ClientArea.AddControl(spnCli);
		tab.Content = scTabCli;
	AddControl(tc);
	// ** Cfg
	umaColor.Reset(sa_sCanales, 5);
		for (j = 0; j < 5; j++)
		{	for (i = 0; i < 4; i++)	umaColor.SetRanges(j, i, -100, 100);	// Usar %
		}
	umaGammaT.Reset(sa_sCanales, 4);//¿3);//cabs de fila: Amplitude, Exponent, Offset
		for (j = 0; j < 3; j++)
		{	for (i = 0; i < 4; i++)	umaGammaT.SetRanges(j, i, -100, 100);	// Usar %
		}
	umaConvolve.Reset(["1", "2", "3"], 3);
		for (j = 0; j < 3; j++)
		{	for (i = 0; i < 3; i++) umaConvolve.SetRanges(j, i, -20, 20, 0.2f);
		}
	umaVert.Reset(["X", "Y", "Z", "W"], 4);
		for (j = 0; j < 3; j++)												// Escalas
		{	for (i = 0; i < 3; i++)	umaVert.SetRanges(j, i, -10, 10, 0.2f);
		}
		for (j = 0; j < 3; j++)	umaVert.SetRanges(j, 3, -0.1f, 0.1f, 0.002f); // Col w.  Los desps se cfg junto con los efs
	r_Commands.Add(new cCommandState(eCmds.EfNuevo, NuevoEf_Execute));
}
// ** Props
public cEffect? EfActual									=>	m_efEfActual;
// ** Mets
public void LimpiaEfectos()									{	m_efEfActual = null; m_p3dActual = null; m_IniEfectos();}
public void Redim()											{	_ = m_tGetEfecto<cScaleEffect>(null, () => new cScaleEffect());}
public void Estira(Point ptTamAnt, Point ptTamActual)
{	cEfEstirar est;

	est = m_tGetEfecto<cEfEstirar>(null, () => new cEfEstirar {	TamIni = ptTamAnt});
	est.Scale = ptTamActual / est.TamIni;
}
public void Recorta(Rectangle rtRegión)
{	cAtlasEffect cre;

	cre = m_tGetEfecto<cAtlasEffect>(null, () => new cAtlasEffect {	Region = new Rectangle(default, rtRegión.Size)});
	cre.Region = new Rectangle(cre.Region.Location + rtRegión.Location, rtRegión.Size); // ** Acumular recorte
}
public void Rota(float fAng, bool bEsRel = false)
{	cEfRot ero;

	ero = m_tGetEfecto<cEfRot>(uslRot, () => new cEfRot());
	if (bEsRel)	ero.Ang += fAng;	else	ero.Ang = fAng;
}
public void Pinta3D(PaintArgs e)							{	m_p3dActual?.Pinta3D(e); }
public void TerminaEdición3D()
{		if (m_p3dActual is null)	return;
	m_p3dActual.TerminaEdición(); m_p3dActual = null;
	u3dObj.Model = null; wMain.VistaActual?.Invalida();
}
private void NuevoEf_Execute(cCommand command, object? args = null)	{	m_efEfActual = null;}
private void uslHueSat_ValueChanged(object sender)
{	cColorMatrixEffect sae;

	sae = m_tGetEfecto<cColorMatrixEffect>(uslHue, () => new cColorMatrixEffect());
	sae.SetHueSaturation(uslHue.Value, uslSat.Value);
}
private void cboColorPresets_SelectionChanged(object sender)
{	string s;

	s = ccbColorPresets.SelectedIndex switch
		{	1	=>	"1; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 0; 0; 0", // Red channel
			2	=>	"0; 0; 0; 0;  0; 1; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 0; 0; 0", // Green channel
			3	=>	"0; 0; 0; 0;  0; 0; 1; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 0; 0; 0", // Blue channel
			4	=>	"1; 0; 0; 0;  0; 1; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 0; 0; 0", // Yellow channel
			5	=>	"1; 0; 0; 0;  0; 0; 0; 0;  0; 0; 1; 0;  0; 0; 0; 1;  0; 0; 0; 0", // Magenta channel
			6	=>	"0; 0; 0; 0;  0; 1; 0; 0;  0; 0; 1; 0;  0; 0; 0; 1;  0; 0; 0; 0", // Cyan channel
			7	=>	"0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  1; 0; 0; 0", // Red
			8	=>	"0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 1; 0; 0", // Green
			9	=>	"0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 0; 1; 0", // Blue
			10	=>	"0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  1; 1; 0; 0", // Yellow
			11	=>	"0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  1; 0; 1; 0", // Magenta
			12	=>	"0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 1; 1; 0", // Cyan
			13	=>	"0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 0; 0; 0", // Black
			14	=>	"0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  1; 1; 1; 0", // White
			15	=>	"0.213; 0.213; 0.213; 0;  0.715; 0.715; 0.715; 0;  0.072; 0.072; 0.072; 0;  0; 0; 0; 1;  0; 0; 0; 0", // Gray scale
			16	=>	"-1; 0; 0; 0;  0; -1; 0; 0;  0; 0; -1; 0;  0; 0; 0; 1;  1; 1; 1; 0", // Invert
			17	=>	"0; 1; 0; 0;  1; 0; 0; 0;  0; 0; 1; 0;  0; 0; 0; 1;  0; 0; 0; 0", // Swap red-green
			18	=>	"0; 0; 1; 0;  0; 1; 0; 0;  1; 0; 0; 0;  0; 0; 0; 1;  0; 0; 0; 0", // Swap red-blue
			19	=>	"1; 0; 0; 0;  0; 0; 1; 0;  0; 1; 0; 0;  0; 0; 0; 1;  0; 0; 0; 0", // Swap green-blue
			20	=>	"0; 0; 0; 0.2125;  0; 0; 0; 0.7154;  0; 0; 0; 0.0721;  0; 0; 0; 0;  0; 0; 0; 0", // Image mask (Luminance to alpha)
			21	=>	"1; 0; 0; 0;  0; 1; 0; 0;  0; 0; 1; 0;  0; 0; 0; -1;  0; 0; 0; 1", // Invert alpha
			_	=>	"1; 0; 0; 0;  0; 1; 0; 0;  0; 0; 1; 0;  0; 0; 0; 1;  0; 0; 0; 0" // Default
		};
	Matrix5x4 m54;	_ = Matrix5x4.TryParse(s, out m54); umaColor.Matrix5x4 = m54 * 100; umaColor_Changed(null);
}
private void umaColor_Changed(object? sender)
{	cColorMatrixEffect cme;

	cme = m_tGetEfecto<cColorMatrixEffect>(umaColor, () => new cColorMatrixEffect());
	cme.Matrix = umaColor.Matrix5x4 * 0.01f;								// Conv a % (puede ser nega)
}
private void umaGammaT_Changed(object sender)
{	cGammaTransferEffect gte; Matrix4x4 m44;

	gte = m_tGetEfecto<cGammaTransferEffect>(umaGammaT, () => new cGammaTransferEffect());
	m44 = umaGammaT.Matrix4x4 * 0.01f;										// Conv a % (puede ser nega)
	gte.RedAmplitude = m44.M00; gte.RedExponent = m44.M10; gte.RedOffset = m44.M20;
	gte.GreenAmplitude = m44.M01; gte.GreenExponent = m44.M11; gte.GreenOffset = m44.M21;
	gte.BlueAmplitude = m44.M02; gte.BlueExponent = m44.M12; gte.BlueOffset = m44.M22;
	gte.AlphaAmplitude = m44.M03; gte.AlphaExponent = m44.M13; gte.AlphaOffset = m44.M23;
}
private void DiscreT_Changed(object sender)
{	cDiscreteTransferEffect dte;

	dte = m_tGetEfecto<cDiscreteTransferEffect>(umaDiscreT, () => new cDiscreteTransferEffect());
	dte.Red = umaDiscreT.Red; dte.Green = umaDiscreT.Green; dte.Blue = umaDiscreT.Blue; dte.Alpha = umaDiscreT.Alpha;
}
private void TableT_Changed(object sender)
{	cTableTransferEffect tte;

	tte = m_tGetEfecto<cTableTransferEffect>(umaTableT, () => new cTableTransferEffect());
	tte.Red = umaTableT.Red; tte.Green = umaTableT.Green; tte.Blue = umaTableT.Blue; tte.Alpha = umaTableT.Alpha;
}
private void Turbu_Changed(object sender)
{	cTurbulenceEffect te;

	te = m_tGetEfecto<cTurbulenceEffect>(uslTurbuFrec, () => new cTurbulenceEffect());
	te.Bounds = new Rectangle(default, (wMain.VistaActual?.Tam).GetValueOrDefault());
	te.BaseFrequency = new Point(uslTurbuFrec.Value, uslTurbuFrec.Value) / 500;
	te.Octaves = (int)uslTurbuOctaves.Value; te.Seed = ntTurbuSeed.IntValue;
	te.Noise = chkTurbuNoise.Checked; te.Stitchable = chkTurbuStitch.Checked;
}
private void uslBrillo_ValueChanged(object sender)
{	cBrightnessEffect bef;

	bef = m_tGetEfecto<cBrightnessEffect>(uslBrilloWX, () => new cBrightnessEffect());
	bef.WhitePoint = new Point(uslBrilloWX.Value, uslBrilloWY.Value); bef.BlackPoint = new Point(uslBrilloBX.Value, uslBrilloBY.Value);
}
private void Contrast_Changed(object sender)
{	cContrastEffect coe;

	coe = m_tGetEfecto<cContrastEffect>(uslContrasContras, () => new cContrastEffect());
	coe.Contrast = uslContrasContras.Value;
}
private void Sharpen_Changed(object sender)
{	cSharpenEffect she;

	she = m_tGetEfecto<cSharpenEffect>(uslSharpenSharp, () => new cSharpenEffect());
	she.Sharpness = uslSharpenSharp.Value; she.Threshold = uslSharpenLim.Value;
}
private void Emboss_Changed(object sender)
{	cEmbossEffect eme;

	eme = m_tGetEfecto<cEmbossEffect>(uslEmbossAlto, () => new cEmbossEffect());
	eme.Height = uslEmbossAlto.Value; eme.Direction = uslEmbossDir.Value;
}
private void EdgeDetection_Changed(object sender)
{	cEdgeDetectionEffect ede;

	ede = m_tGetEfecto<cEdgeDetectionEffect>(uslEdgeStrength, () => new cEdgeDetectionEffect());
	ede.Strength = uslEdgeStrength.Value; ede.BlurRadius = uslEdgeBlur.Value;
	ede.Mode = (cEdgeDetectionEffect.eMode)ccbEdgeMode.SelectedIndex; ede.OverlayEdges = chkEdgeOverlay.Checked;
}
private void Vigne_Changed(object sender)
{	cVignetteEffect she;

	she = m_tGetEfecto<cVignetteEffect>(clbVigneColor, () => new cVignetteEffect());
	she.Color = clbVigneColor.Color; she.TransitionSize = uslVigneTran.Value; she.Strength = uslVigneStren.Value;
}
//private void Chr_Changed(object sender)
//{	cChromaKeyEffect she;

//	she = m_tGetEfecto<cChromaKeyEffect>(clbChrColor, () => new cChromaKeyEffect());
//	she.Color = clbChrColor.Color; she.Tol = uslChrTol.Value; she.Inv = chkChrInv.Checked; she.Fea = chkChrFea.Checked;
//}
private void uslGauss_ValueChanged(object sender)
{	cGaussianBlurEffect gbe;

	gbe = m_tGetEfecto<cGaussianBlurEffect>(uslGauss, () => new cGaussianBlurEffect());
	gbe.Deviation = uslGauss.Value * 10;
}
private void uslDirBlur_ValueChanged(object sender)
{	cDirectionalBlurEffect dbe;

	dbe = m_tGetEfecto<cDirectionalBlurEffect>(uslDirBlurDesv, () => new cDirectionalBlurEffect());
	dbe.Deviation = uslDirBlurDesv.Value * 10; dbe.Angle = uslDirBlurAng.Value * 180;
}
private void cboConvolvePresets_SelectionChanged(object sender)
{	string s;

	uslConvolveDivi.Value = 1; uslConvolveBias.Value = 0;
	switch (ccbConvolvePresets.SelectedIndex)///buscar vals
	{	case 1:	s = "1; 2; 1;  2; 2; 2;  1; 2; 1";							// Blur
			uslConvolveDivi.Value = 14;
			break;
		case 2:	s = "-1; -1; -1;  -1; 9; -1;  -1; -1; -1";					// Sharpen
			break;
		case 3:	s = "-2.2; -1.4; -0.4;  -1; 9; -1;  -1; -1; -1";			// Edge detection
			uslConvolveBias.Value = -0.4f;
			break;
		case 4:	s = "-2.2; -1.4; -0.4;  -1; 9; -1;  -1; -1; -1";			// Emboss
			uslConvolveBias.Value = 0.9f;
			break;
		default:	s = "0; 0; 0;  0; 1; 0;  0; 0; 0";						// Default
			break;
	}
	Matrix3x3 m33;	_ = Matrix3x3.TryParse(s, out m33); umaColor.Matrix3x3 = m33; Convolve_Changed(null);
}
private void Convolve_Changed(object? sender)
{	cConvolveMatrixEffect coe; Matrix3x3 m33;

	coe = m_tGetEfecto<cConvolveMatrixEffect>(umaConvolve, () => new cConvolveMatrixEffect());
	m33 = umaConvolve.Matrix3x3;
	coe.Matrix = [m33.M00, m33.M01, m33.M02, m33.M10, m33.M11, m33.M12, m33.M20, m33.M21, m33.M22];
	coe.Offset = uptConvolveDesp.Value; coe.Divisor = uslConvolveDivi.Value;
	coe.Bias = uslConvolveBias.Value; coe.PreserveAlpha = chkConvolveAlfa.Checked;
}
private void ConvolveAbrir_Click(object sender, out bool handled)
{	string? s; System.Xml.XmlDocument xd; System.Xml.XmlElement xe;

	handled = true;
	try
	{	s = mDialog.ShowOpenFile("Convolve matrix|*.convolvemx|All files|*.*", new System.Guid("{7BADF31B-94E4-4216-A9D2-CF9F5EC94E2C}"));
			if (s is null) return;											// Cancelado: salir
		xd = new System.Xml.XmlDocument(); xd.Load(s); xe = xd["convolvemx"]!;
		Matrix3x3 m33;	_ = Matrix3x3.TryParse(xe.Attributes["value"]!.Value, out m33); umaConvolve.Matrix3x3 = m33;
		Point pt;	_ = Point.TryParseInvariant(xe.Attributes["offset"]!.Value, out pt); uptConvolveDesp.Value = pt;
		uslConvolveDivi.Value = xe.Attributes["divisor"]!.Value.ToFloatInvariant();
		uslConvolveBias.Value = xe.Attributes["bias"]!.Value.ToFloatInvariant();
	} catch (System.Exception ex)
	{	mDialog.MsgBoxError(ex);
	}
}
private void ConvolveGrabar_Click(object sender, out bool handled)
{	string? s; System.Xml.XmlWriter? xw = null;

	handled = true;
	try
	{	s = mDialog.ShowSaveFile("Convolve matrix|*.convolvemx", new System.Guid("{7BADF31B-94E4-4216-A9D2-CF9F5EC94E2C}"));
			if (s is null) return;											// Cancelado: salir
		xw = System.Xml.XmlWriter.Create(s, new System.Xml.XmlWriterSettings {	Indent = true, IndentChars = "\t"});
		xw.WriteStartElement("convolvemx");
			xw.WriteAttributeString("value", umaConvolve.Matrix3x3.ConvertToString());
			xw.WriteAttributeString("offset", uptConvolveDesp.Value.ConvertToString());
			xw.WriteAttributeString("divisor", uslConvolveDivi.Value.ToStringInvariant());
			xw.WriteAttributeString("bias", uslConvolveBias.Value.ToStringInvariant());
	} catch (System.Exception ex)
	{	mDialog.MsgBoxError(ex);
	} finally
	{	xw?.Dispose();
	}
}
private void Thick_Changed(object sender)
{	cThicknessEffect tie;

	tie = m_tGetEfecto<cThicknessEffect>(chkThickThick, () => new cThicknessEffect());
	tie.Thick = chkThickThick.Checked;
	tie.KernelWidth = (int)uslThickKAncho.Value; tie.KernelHeight = (int)uslThickKAlto.Value;
}
private void Displace_Changed(object sender)
{	cDisplacementMapEffect dme;

		if (!uoDisplaceOrig.Valida("Set a map first", "Displacement effect"))	return; // Sin map: salir
	dme = m_tGetEfecto<cDisplacementMapEffect>(uoDisplaceOrig, () => new cDisplacementMapEffect(), true);
	dme.Map = uoDisplaceOrig.Value; dme.Scale = uslDisplaceEscala.Value;
	dme.XChannel = (cDisplacementMapEffect.eChannel)ccbDisplaceX.SelectedIndex;
	dme.YChannel = (cDisplacementMapEffect.eChannel)cboDisplaceY.SelectedIndex;
}
private void Dft_Changed(object sender)
{	cDFTEffect dft;

	dft = m_tGetEfecto<cDFTEffect>(chkThickThick, () => new cDFTEffect());
}
private void Shadow_Changed(object sender)
{	cShadowEffect se;

	se = m_tGetEfecto<cShadowEffect>(uslShadowDesv, () => new cShadowEffect());
	se.BlurDeviation = uslShadowDesv.Value * 10; se.Offset = uptShadowOffset.Value; se.Color = clbShadowColor.Color;
}
private void Make3D_Changed(object sender)
{	cMake3DEffect e3d;

	e3d = m_tGetEfecto<cMake3DEffect>(leMake3D, () => new cMake3DEffect());
	leMake3D.AplicaEfecto(e3d); e3d.Thickness = uslMake3DThickness.Value; 
}
private void PointSpec_Changed(object sender)
{	cPointSpecularEffect pse;

	pse = m_tGetEfecto<cPointSpecularEffect>(lePointSpec, () => new cPointSpecularEffect());
	lePointSpec.AplicaEfecto(pse); pse.LightPosition = lePointSpec.PosLuz; pse.ShineFocus = lePointSpec.Shine;
}
private void PointDiff_Changed(object sender)
{	cPointDiffuseEffect pde;

	pde = m_tGetEfecto<cPointDiffuseEffect>(lePointDiff, () => new cPointDiffuseEffect());
	lePointDiff.AplicaEfecto(pde); pde.LightPosition = lePointDiff.PosLuz;
}
private void SpotSpec_Changed(object sender)
{	cSpotSpecularEffect sse;

	sse = m_tGetEfecto<cSpotSpecularEffect>(lesSpotSpec, () => new cSpotSpecularEffect());
	lesSpotSpec.AplicaEfecto(sse); sse.LightPosition = lesSpotSpec.PosLuz; sse.PointsAt = lesSpotSpec.Apunta;
	sse.Focus = lesSpotSpec.Foco; sse.ConeAngle = lesSpotSpec.Cono; sse.ShineFocus = lesSpotSpec.Shine;
}
private void SpotDiff_Changed(object sender)
{	cSpotDiffuseEffect sde;

	sde = m_tGetEfecto<cSpotDiffuseEffect>(lesSpotDiff, () => new cSpotDiffuseEffect());
	lesSpotDiff.AplicaEfecto(sde); sde.LightPosition = lesSpotDiff.PosLuz; sde.PointsAt = lesSpotDiff.Apunta;
	sde.Focus = lesSpotDiff.Foco; sde.ConeAngle = lesSpotDiff.Cono;
}
private void DistSpec_Changed(object sender)
{	cDistantSpecularEffect dde;

	dde = m_tGetEfecto<cDistantSpecularEffect>(ledDistSpec, () => new cDistantSpecularEffect());
	ledDistSpec.AplicaEfecto(dde); dde.Azimuth = ledDistSpec.Acimut;
	dde.Elevation = ledDistSpec.Eleva; dde.ShineFocus = ledDistSpec.Shine;
}
private void DistDiff_Changed(object sender)
{	cDistantDiffuseEffect dde;

	dde = m_tGetEfecto<cDistantDiffuseEffect>(ledDistDiff, () => new cDistantDiffuseEffect());
	ledDistDiff.AplicaEfecto(dde); dde.Azimuth = ledDistDiff.Acimut; dde.Elevation = ledDistDiff.Eleva;
}
private void uslRot_ValueChanged(object sender)				{	Rota(uslRot.Value);}
private void Flip_CheckStateChanged(object sender)
{	cEfFlip efl;

	efl = m_tGetEfecto<cEfFlip>(chkFlipV, () => new cEfFlip());
	efl.Cfg(chkFlipH.Checked, chkFlipV.Checked);
}
private void cboVertPresets_SelectionChanged(object sender)
{	Matrix4x4 m44; Point pt = (wMain.VistaActual?.Tam).GetValueOrDefault();

	switch (ccbVertPresets.SelectedIndex)
	{	case 1:	_ = Matrix4x4.TryParse("2; 0; 0; 0;  0; 2; 0; 0;  0; 0; 1; 0;  0; 0; 0; 1", out m44); break; // Stretch
		case 2: _ = Matrix4x4.TryParse("0.5; 0; 0; 0;  0; 0.5; 0; 0;  0; 0; 1; 0;  0; 0; 0; 1", out m44); break; // Shrink
		case 3: m44 = Matrix4x4.Identity; m44.M30 = pt.X * 0.1f; m44.M31 = pt.Y * 0.1f; break; // Offset
		case 4: m44 = Matrix4x4.Identity; m44.M10 = 1; m44.M30 = -pt.X * 0.5f; break; // SkewX
		case 5: m44 = Matrix4x4.Identity; m44.M01 = 1; m44.M31 = -pt.Y * 0.5f; break; // SkewY
		case 6: _ = Matrix4x4.TryParse("1; 0; 0; 0.005;  0; 1; 0; 0;  0; 0; 1; 0;  0; 0; 0; 1", out m44); break; // Yaw
		case 7:	_ = Matrix4x4.TryParse("1; 0; 0; 0;  0; 1; 0; 0.02;  0; 0; 1; 0;  0; 0; 0; 1", out m44); break; // Pitch
		case 8:	m44 = Matrix4x4.Identity; m44.M00 = -1; m44.M30 = pt.X; break; // Flip horizontal
		case 9:	m44 = Matrix4x4.Identity; m44.M11 = -1; m44.M31 = pt.Y; break; // Flip vertical
		default:	m44 = Matrix4x4.Identity; break;						// Deafult
	}
	umaVert.Matrix4x4 = m44; umaVert_Changed(null);
}
private void umaVert_Changed(object? sender)
{	c3DTransformEffect _3e;

	_3e = m_tGetEfecto<c3DTransformEffect>(umaVert, () => new c3DTransformEffect());
	_3e.Matrix = umaVert.Matrix4x4;
}
private void Tile_Changed(object sender)
{	cTileEffect tle; fVista vis = wMain.VistaActual;

		if (vis is null)	return;
	tle = m_tGetEfecto<cTileEffect>(ccbTileModoX
		, () =>
		{	cTileEffect tle2;
		
			tle2 = new cTileEffect();
			vis.IniConSelEf(new Rectangle(default, vis.Tam / 2), () => {	tle2.Rectangle = vis.PlaceSel; vis.Invalida();});
			return tle2;
		}
		, true);
	tle.ModeX = (eExtendMode)ccbTileModoX.SelectedIndex; tle.ModeY = (eExtendMode)ccbTileModoY.SelectedIndex;
}
private void Wave_Changed(object sender)
{	cWaveEffect wav;

	wav = m_tGetEfecto<cWaveEffect>(uslWaveDesp, () => new cWaveEffect());
	wav.Offset = uslWaveDesp.Value; wav.Skew = new Point(uslWaveSkewX.Value, uslWaveSkewY.Value);
}
private void Ripple_Changed(object sender)
{	cRippleEffect rip;

	rip = m_tGetEfecto<cRippleEffect>(uslRippleFrec, () => new cRippleEffect());
	rip.Frequency = uslRippleFrec.Value; rip.Phase = uslRippleFase.Value; rip.Amplitude = uslRippleAmp.Value;
	rip.Spread = uslRippleSpread.Value; rip.Center = uptRippleCenter.Value; rip.Limit = uslRippleLim.Value;
}
private void Bend_Changed(object sender)
{	cBendEffect ben;

	ben = m_tGetEfecto<cBendEffect>(uslBendAng, () => new cBendEffect());
	ben.AxisAngle = uslBendAng.Value; ben.Distance = uslBendDist.Value; ben.Radius = uslBendRadio.Value;
}
private void Whirl_Changed(object sender)
{	cWhirlEffect wir;

	wir = m_tGetEfecto<cWhirlEffect>(uptWhirlCenter, () => new cWhirlEffect());
	wir.Center = uptWhirlCenter.Value; wir.Radius = uslWhirlRadio.Value; wir.Spin = uslWhirlSpin.Value;
}
private void Bubble_Changed(object sender)
{	cBubbleEffect bub;

	bub = m_tGetEfecto<cBubbleEffect>(uptBubbleCenter, () => new cBubbleEffect());
	bub.Center = uptBubbleCenter.Value; bub.Radius = uslBubbleRadio.Value;
	bub.Flatten = uslBubbleFlatten.Value; bub.Falloff = uslBubbleFalloff.Value; bub.Light = uslBubbleLuz.Value;
}
private void Blend_Changed(object sender)
{	cBlendEffect be;

		if (!uoBlendOrig.Valida("Set a source first", "Blend effect"))	return; // Sin orig: salir
	be = m_tGetEfecto<cBlendEffect>(uoBlendOrig, () => new cBlendEffect(), true);
	be.Source = uoBlendOrig.Value; be.Mode = (cBlendEffect.eBlend)ccbBlendModo.SelectedIndex;
}
private void Composi_Changed(object sender)
{	cCompositeEffect ce;

		if (!uoComposiOrig.Valida("Set a source first", "Composition effect"))	return; // Sin orig: salir
	ce = m_tGetEfecto<cCompositeEffect>(uoComposiOrig, () => new cCompositeEffect(), true);
	ce[1] = uoComposiOrig.Value; ce.Mode = (eComposite)ccbComposiModo.SelectedIndex;
}
private void Arit_Changed(object sender)
{	cArithmeticBlendEffect ae;

		if (!uoAritOrig.Valida("Set a source first", "Arithmetic blend effect"))	return; // Sin orig: salir
	ae = m_tGetEfecto<cArithmeticBlendEffect>(uoAritOrig, () => new cArithmeticBlendEffect(), true);
	ae.Source = uoAritOrig.Value;
	ae.DestinationxSourceFactor = uslAritDestxSource.Value / 100; ae.DestinationFactor = uslAritDest.Value / 100;
	ae.SourceFactor = uslAritSource.Value / 100; ae.OffsetFactor = uslAritOffset.Value / 100;
}
private void tbbColorKeyMuestra_Click(object sender)
{	tbbColorKeyMuestra.Captured = true; mMouse.Global = mRes.CurMuestra;
}
private void tbbColorKeyMuestra_MouseDown(object sender, MouseArgs e)
{	cSolidBrush? sbr;

		if (mMouse.Global! != mRes.CurMuestra)	return;
	tbbColorKeyMuestra.Captured = false; mMouse.Global = null;
	sbr = wMain.VistaActual?.TomaMuestra(tbbColorKeyMuestra.PointToScreen(e.Location), true);	if (sbr is null)	return;
	clbColorKeyColor.Color = sbr.Color; sbr.Dispose();
	ColorKey_Changed(null);
}
private void ColorKey_Changed(object? sender)
{	cColorKeyEffect cke;

	cke = m_tGetEfecto<cColorKeyEffect>(uoColorKeyFondo, () => new cColorKeyEffect(), true);
	cke.Background = uoColorKeyFondo.Value; cke.BackgroundColor = clbColorKeyFondoColor.Color;
	cke.Color = clbColorKeyColor.Color; cke.Tolerance = uslColorKeyTolera.Value / 1_000;
}
private void tbt3DObjAbrir_Click(object sender)
{	string? s;
	
	s = mDialog.ShowOpenFile("3D models|*.mdl|All files|*.*", new System.Guid("{CE83EF0E-9CB1-4342-B716-467098272A68}"));
		if (s is null) return;
	m_3DObjAgrega(new c3DModel(mMod.MainWnd, s), (int)new System.IO.FileInfo(s).Length);
}
private void tbt3DObjPegar_Click(object sender)
{	c3DModel? mdl;
	
	mdl = mClipboard.Get3DModel(mMod.MainWnd);	if (mdl is null)	{	mDialog.MsgBoxExclamation("Invalid format.", "Paste"); return;}
	m_3DObjAgrega(mdl, 100000);//¿tam
}
private void u3dObj_Changed(object sender)					{	m_p3dActual!.Vista.Changed = true;}
private void u3dObj_ModelInvalidated(object sender)			{	m_p3dActual!.Vista.Invalida();}
private void m_3DObjAgrega(c3DModel mdlMdl, int iTam)
{	fVista vis = wMain.VistaActual;

		if (vis is null)	return;
try
{	m_efEfActual = null; m_IniEfectos(u3dObj);
	if (m_p3dActual is null)	{	m_p3dActual = new cPrimObj3D(vis); vis.AddPrim(m_p3dActual);}
	m_p3dActual.Mdl = mdlMdl; u3dObj.Model = mdlMdl;
	vis.RedimÍtemUndo(m_p3dActual, iTam);
} catch (System.Exception ex)
{	mDialog.MsgBoxError(ex);
}
	vis.Changed = true;
}
private void m_IniEfectos(cControl? ctlExcepto = null)
{	if (ctlExcepto != uslHue)			{	uslHue.Value = 0; uslSat.Value = 1;}
	if (ctlExcepto != umaColor)		{	ccbColorPresets.SelectedIndex = 0; umaColor.Matrix5x4 = Matrix5x4.Identity * 100;} // Conv a %
	if (ctlExcepto != umaGammaT)
	{	umaGammaT.Matrix4x4 = new Matrix4x4(100, 100, 100, 100, 100, 100, 100, 100, 0, 0, 0, 0, 0, 0, 0, 0);
	}
	if (ctlExcepto != umaDiscreT)			umaDiscreT.Reset();
	if (ctlExcepto != umaTableT)			umaTableT.Reset();
	if (ctlExcepto != uslTurbuFrec)
	{	uslTurbuFrec.Value = 0.01f * 500; uslTurbuOctaves.Value = 1; ntTurbuSeed.IntValue = 0;
		chkTurbuNoise.Checked = true; chkTurbuStitch.Checked = false;
	}
	if (ctlExcepto != uslBrilloWX)	{	uslBrilloWX.Value = 1; uslBrilloWY.Value = 1; uslBrilloBX.Value = 0; uslBrilloBY.Value = 0;}
	if (ctlExcepto != uslContrasContras)	uslContrasContras.Value = 0;
	if (ctlExcepto != uslSharpenSharp)	{	uslSharpenSharp.Value = 0; uslSharpenLim.Value = 0;}
	if (ctlExcepto != uslEmbossAlto)	{	uslEmbossAlto.Value = 1; uslEmbossDir.Value = 0;}
	if (ctlExcepto != uslEdgeStrength)
	{	uslEdgeStrength.Value = 1; uslEdgeBlur.Value = 0; ccbEdgeMode.SelectedIndex = 0; chkEdgeOverlay.Checked = false;
	}
	if (ctlExcepto != clbVigneColor)	{	clbVigneColor.Color = eColor.White; uslVigneTran.Value = 0; uslVigneStren.Value = 0;}
	//¿if (ctlExcep != clbChrColor)	{	clbChrColor.Color = eColor.White; uslChrTol.Value = 0; chkChrInv.Checked = false; chkChrFea.Checked = false;}
	if (ctlExcepto != uslGauss)		{	uslGauss.Value = 0;}
	if (ctlExcepto != uslDirBlurDesv)	{	uslDirBlurDesv.Value = 0; uslDirBlurAng.Value = 0;}
	if (ctlExcepto != umaConvolve)
	{	ccbConvolvePresets.SelectedIndex = 0; umaConvolve.Matrix3x3 = new Matrix3x3(0, 0, 0, 0, 1, 0, 0, 0, 0);
		uptConvolveDesp.Value = default; uslConvolveDivi.Value = 1; uslConvolveBias.Value = 0; chkConvolveAlfa.Checked = true;
	}
	if (ctlExcepto != chkThickThick)	{	chkThickThick.Checked = false; uslThickKAncho.Value = 0; uslThickKAlto.Value = 0;}
	if (ctlExcepto != uoDisplaceOrig)
	{	uoDisplaceOrig.Value = null; uslDisplaceEscala.Value = 0; ccbDisplaceX.SelectedIndex = 3; cboDisplaceY.SelectedIndex = 3;
	}
	if (ctlExcepto != uslShadowDesv)
	{	uslShadowDesv.Value = 0; uptShadowOffset.Value = new Point(5, 5); clbShadowColor.Color = eColor.Black;
	}
	if (ctlExcepto != leMake3D)		{	leMake3D.IniEfectoMake3D(); uslMake3DThickness.Value = 3;}
	if (ctlExcepto != lePointSpec)		lePointSpec.IniEfecto();
	if (ctlExcepto != lePointDiff)		lePointDiff.IniEfecto();
	if (ctlExcepto != lesSpotSpec)		lesSpotSpec.IniEfecto();
	if (ctlExcepto != lesSpotDiff)		lesSpotDiff.IniEfecto();
	if (ctlExcepto != ledDistSpec)		ledDistSpec.IniEfecto();
	if (ctlExcepto != ledDistDiff)		ledDistDiff.IniEfecto();
	if (ctlExcepto != uslRot)				uslRot.Value = 0;
	if (ctlExcepto != chkFlipV)		{	chkFlipV.Checked = false; chkFlipH.Checked = false;}
	if (ctlExcepto != umaVert)
	{	float f;
	
		ccbVertPresets.SelectedIndex = 0;
		f = (wMain.VistaActual?.Tam).GetValueOrDefault(Point.Identity).GetMaxComponent().X;
			for (int i = 0; i < 3; i++)	umaVert.SetRanges(3, i, -f, f, f / 50); // Desps
		umaVert.Matrix4x4 = Matrix4x4.Identity;
	}
	if (ctlExcepto != ccbTileModoX)	{	ccbTileModoX.SelectedIndex = 1; ccbTileModoY.SelectedIndex = 1;}
	if (ctlExcepto != uslWaveDesp)	{	uslWaveDesp.Value = 0; uslWaveSkewX.Value = 0; uslWaveSkewY.Value = 0;}
	if (ctlExcepto != uslRippleFrec)
	{	uslRippleFrec.Value = 30; uslRippleFase.Value = 0; uslRippleAmp.Value = 800;
		uslRippleSpread.Value = 10; uptRippleCenter.Value = new Point(0.5f, 0.5f); uslRippleLim.Value = 0.7f;
	}
	if (ctlExcepto != uslBendAng)		{	uslBendAng.Value = 45; uslBendDist.Value = 1; uslBendRadio.Value = 0.1f;}
	if (ctlExcepto != uptWhirlCenter)
	{	uptWhirlCenter.Value = new Point(0.5f, 0.5f); uslWhirlRadio.Value = 1; uslWhirlSpin.Value = 0;
	}
	if (ctlExcepto != uptBubbleCenter)
	{	uptBubbleCenter.Value = new Point(0.5f, 0.5f); uslBubbleRadio.Value = 0;
		uslBubbleFlatten.Value = 0.5f; uslBubbleFalloff.Value = 10; uslBubbleLuz.Value = 0;
	}
	if (ctlExcepto != uoBlendOrig)	{	uoBlendOrig.Value = null; ccbBlendModo.SelectedIndex = 0;}
	if (ctlExcepto != uoComposiOrig)	{	uoComposiOrig.Value = null; ccbComposiModo.SelectedIndex = 0;}
	if (ctlExcepto != uoAritOrig)
	{	uoAritOrig.Value = null;
		uslAritDestxSource.Value = 50; uslAritDest.Value = 50; uslAritSource.Value = 50; uslAritOffset.Value = 50;
	}
	if (ctlExcepto != uoColorKeyFondo)
	{	uoColorKeyFondo.Value = null; clbColorKeyFondoColor.Color = eColor.White;
		clbColorKeyColor.Color = eColor.Green; uslColorKeyTolera.Value = 50;
	}
	if (ctlExcepto != u3dObj)				u3dObj.Model = null;
}
private T m_tGetEfecto<T>(cControl? ctlExcepto, dCreaEf ceCreaEf, bool bMantenerSelEf = false) where T : cEffect
{	fVista vis = wMain.VistaActual;

		if (vis is null)													// Sin vis: salir
		{	if (m_efEfActual?.GetType() != typeof(T))	m_efEfActual = ceCreaEf(); // No devolver null
			return (T)m_efEfActual;
		}
	if (!bMantenerSelEf || !vis.ModoSelEf) wMain.TerminaEdición();			// ** Limpiar sel si no se pide mantener
	m_p3dActual = null; m_IniEfectos(ctlExcepto);							// Limpiar los demás efs
	if (m_efEfActual?.GetType() != typeof(T))								// ** El ef actual no es del tp exacto: crear ef del nuevo tp
	{	cPrimCmdLs cml;
	
		cml = new cPrimCmdLs(vis, ceCreaEf()); m_efEfActual = cml.Efecto; vis.AddPrim(cml); // Agregar ef
	}
	vis.Changed = true;
	return (T)m_efEfActual;
}
}