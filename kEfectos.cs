using Wew.Control;
using Wew.Media;
using Wew;

namespace DirectPaint
{
class kEfectos : cDockControl
{// ** Tps
	class cEfEstirar : cScaleEffect
	{	public Point TamIni;
	}
	class cEfRot : c3DTransformEffect										// Sólo para rotar
	{	float m_fAng;
		public float Ang
		{	get												{	return m_fAng;}
			set
			{	Point pt = (wMain.VistaActual?.Tam).GetValueOrDefault();
				Matrix = Matrix4x4.Rotation(0, 0, value, new Vector(pt.X / 2, pt.Y / 2, 0));
				m_fAng = value;
			}
		}
	}
	class cEfFlip : c3DTransformEffect										// Sólo para hacer flip
	{	public void Cfg(bool bHoriz, bool bVert)
		{	Matrix4x4 mat = Matrix4x4.Identity; Point pt = (wMain.VistaActual?.Tam).GetValueOrDefault();

			if (bHoriz) {	mat._11 = -1; mat._41 = pt.X;}
			if (bVert)	{	mat._22 = -1; mat._42 = pt.Y;}
			Matrix = mat;
		}
	}
	private class cCbo : uPropertySubgroup
	{	cComboBox cboCbo;
		public event dEvent SelectionChanged
		{	add												{	cboCbo.SelectionChanged += value;}
			remove											{	cboCbo.SelectionChanged -= value;}
		}
		public cCbo(float fCboAncho = 0)
		{	cboCbo = new cComboBox();	if (fCboAncho != 0)	cboCbo.Width = fCboAncho;	else	cboCbo.RightMargin = 0;
				AddControl(cboCbo);
		}
		public System.Collections.IEnumerable Items
		{	get												{	return cboCbo.Data;}
			set												{	cboCbo.Data = value;}
		}
		public int SelectedIndex
		{	get												{	return cboCbo.SelectedIndex;}
			set												{	cboCbo.SelectedIndex = value;}
		}
	}
	private class uEfOrig : uPropertySubgroup
	{	c3DTransformEffect m_3deValue;
		cPicture picImg;
		public event dEvent Changed;
		public uEfOrig()
		{	cToolButton tbt;

			Text = "Source";
			tbt = new cToolButton() {	Bitmap = mRes.BmpAbrir, ToolTip = "Open"};
				tbt.Click += Abrir_Click; AddControl(tbt);
			tbt = new cToolButton() {	Bitmap = mRes.BmpPegar, ToolTip = "Paste"};
				tbt.Click += Pegar_Click; AddControl(tbt);
			picImg = new cPicture() {	Width = 100, Height = 69};
				AddControl(picImg);
		}
		public c3DTransformEffect Value
		{	get												{	return m_3deValue;}
			set												{	m_3deValue = value; picImg.Bitmap = null;}
		}
		public bool Valida(string sMsj, string sTit)
		{	if (m_3deValue == null)	{	mDialog.MsgBoxExclamation(sMsj, sTit); Focus(); return false;} // Sin orig: salir
			return true;
		}
		private void Abrir_Click(object sender)
		{	string s;
	
			s = mDialog.ShowOpenFile(mMod.FILTRO_ABRIR, mMod.DLG_EF_GUID);	if (s == null) return;
		try
		{	picImg.Bitmap = new cBitmap(s); m_Carga(picImg.Bitmap);
		} catch (System.Exception ex)
		{	mDialog.MsgBoxExclamation(ex.Message, "Error");
		}
		}
		private void Pegar_Click(object sender)
		{	cBitmap bmp;

			bmp = mClipboard.GetImage();	if (bmp == null)	return;
			picImg.Bitmap = bmp; m_Carga(picImg.Bitmap); 
		}
		private void m_Carga(cBitmap bmpOrig)
		{	fVista vis = wMain.VistaActual;

				if (vis == null)	return;
			m_3deValue = new c3DTransformEffect(); m_3deValue.Input = bmpOrig;
			vis.IniConSelEf(new Rectangle(Point.Zero, vis.Tam)
				, () =>
				{	m_3deValue.SetRectangle(vis.PlaceSelReal.Location, vis.PlaceSelReal.Size / bmpOrig.Size);
					mMod.Efectos.EfActual?.Invalidate();
				});
			Changed?.Invoke(this);
		}
	}
	private class uTransform : uPropertyGroup
	{	Matrix5x4 m_m54Mat;
		cCheckBox chkR, chkG, chkB, chkA; uMatrix umaMat;
		public event dEvent Changed;
		public uTransform()
		{	cContainer cnt;

			cnt = new cContainer() {	AutoSize = eAutoSize.Height};
				chkR = new cCheckBox() {	Text = "Red", Checked = true};
					chkR.CheckStateChanged += Ctl_Changed; cnt.AddControl(chkR);
				chkG = new cCheckBox() {	Text = "Green", LeftMargin = 85, Checked = true};
					chkG.CheckStateChanged += Ctl_Changed; cnt.AddControl(chkG);
				chkB = new cCheckBox() {	Text = "Blue", LeftMargin = 170, Checked = true};
					chkB.CheckStateChanged += Ctl_Changed; cnt.AddControl(chkB);
				chkA = new cCheckBox() {	Text = "Alpha", LeftMargin = 255};
					chkA.CheckStateChanged += Ctl_Changed; cnt.AddControl(chkA);
				AddControl(cnt);
			umaMat = new uMatrix() {	Height = 155}; umaMat.Changed += Ctl_Changed;
				umaMat.Initialize(new string[] {	"Red", "Green", "Blue", "Alpha"}, 5);
				for (int j = 0; j < 5; j++)	for (int i = 0; i < 4; i++)	umaMat.SetRanges(j, i, -100, 100); // Usar %
				AddControl(umaMat);
		}
		public float[] Red
		{	get
			{	return (chkR.Checked ? new float[] {	m_m54Mat._11, m_m54Mat._21, m_m54Mat._31, m_m54Mat._41, m_m54Mat._51} : null);
			}
		}
		public float[] Green
		{	get
			{	return (chkG.Checked ? new float[] { m_m54Mat._12, m_m54Mat._22, m_m54Mat._32, m_m54Mat._42, m_m54Mat._52} : null);
			}
		}
		public float[] Blue
		{	get
			{	return (chkB.Checked ? new float[] { m_m54Mat._13, m_m54Mat._23, m_m54Mat._33, m_m54Mat._43, m_m54Mat._53} : null);
			}
		}
		public float[] Alpha
		{	get
			{	return (chkA.Checked ? new float[] { m_m54Mat._14, m_m54Mat._24, m_m54Mat._34, m_m54Mat._44, m_m54Mat._54} : null);
			}
		}
		public void Reset()
		{	umaMat.Matrix5x4 = new Matrix5x4(0, 0, 0, 0, 25, 25, 25, 25, 50, 50, 50, 50, 75, 75, 75, 75, 100, 100, 100, 100);
			m_m54Mat = umaMat.Matrix5x4 * 0.01f;							// Conv a % (puede ser nega)
		}
		private void Ctl_Changed(object sender)
		{	if (sender == umaMat)	m_m54Mat = umaMat.Matrix5x4 * 0.01f;	// Conv a % (puede ser nega)
			Changed?.Invoke(this);
		}
	}
	private class uEfLuz : uPropertyGroup
	{	uSlider uslZEscala, uslReflejo; 
		uVector uvPosLuz; uSlider uslShine;									// Opcionales
		cColorButton clbColor; uPoint uptUniLong; cCbo ccbInterpol;		// Opcionales
		public event dEvent Changed;
		public uEfLuz(bool bPosLuz = true, bool bShine = true, bool bEsMake3D = false)
		{	uPropertySubgroup sgr;

			AutoSize = eAutoSize.Height;
			// ** Opcio
			if (bPosLuz)
			{	uvPosLuz = new uVector() {	Text = "Light position"};
					uvPosLuz.ValueChanged += Ctl_Changed; AddControl(uvPosLuz);
			}
			if (bShine)
			{	uslShine = new uSlider() {	Text = "Shine focus", Maximum = 3};
					uslShine.ValueChanged += Ctl_Changed; AddControl(uslShine);
			}
			// ** Fijos
			uslZEscala = new uSlider() {	Text = "Z scale", Maximum = 30};
				uslZEscala.ValueChanged += Ctl_Changed; AddControl(uslZEscala);
			uslReflejo = new uSlider() {	Text = "Reflection", Maximum = 1};
				uslReflejo.ValueChanged += Ctl_Changed; AddControl(uslReflejo);
			if (!bEsMake3D)
			{	sgr = new uPropertySubgroup() {	Text = "Color"};
					clbColor = new cColorButton();
						clbColor.ColorChanged += Ctl_Changed; sgr.AddControl(clbColor);
					AddControl(sgr);
				uptUniLong = new uPoint() {	Text= "Unit length"};
					uptUniLong.ValueChanged += Ctl_Changed; AddControl(uptUniLong);
				ccbInterpol = new cCbo()
						{	Items = new string[] {	"Nearest neighbor", "Linear", "Cubic"
								, "MultisampleLinear", "Anisotropic", "HighQualityCubic"}
							, SelectedIndex = 0, Text = "Interpolation" 
						};
					ccbInterpol.SelectionChanged += Ctl_Changed; AddControl(ccbInterpol);
			}
		}
		public Vector PosLuz
		{	get												{	return uvPosLuz.Value;}
		}
		public float Shine
		{	get												{	return uslShine.Value;}
		}
		public virtual void IniEfecto()
		{	if (uvPosLuz != null)	uvPosLuz.Value = Vector.Zero;
			if (uslShine != null)	uslShine.Value = 0.8f;
			uslZEscala.Value = 10; uslReflejo.Value = 0.8f;
			if (clbColor != null)
			{	clbColor.Color = eColor.White; uptUniLong.Value = new Point(1, 1);
				ccbInterpol.SelectedIndex = (int)eInterpolation.Linear;
			}
		}
		public void IniEfectoMake3D()						{	IniEfecto(); uslZEscala.Value = 3; uslShine.Value = 2.5f;}
		public void AplicaEfecto(cLightEffect leEf)
		{	leEf.ZScale = uslZEscala.Value; leEf.Reflection = uslReflejo.Value;
			leEf.Color = clbColor.Color; leEf.KernelUnitLength = uptUniLong.Value;
			leEf.ScaleMode = (eInterpolation)ccbInterpol.SelectedIndex;
		}
		public void AplicaEfecto(cMake3DEffect e3dEf)
		{	e3dEf.ZScale = uslZEscala.Value; e3dEf.Reflection = uslReflejo.Value;
			e3dEf.LightPosition = uvPosLuz.Value; e3dEf.ShineFocus = uslShine.Value;
		}
		protected void Ctl_Changed(object sender)			{	Changed?.Invoke(this);}
	}
	private class uLuzSpot : uEfLuz
	{	uVector uvApunta; uSlider uslFoco, uslCono;
		public uLuzSpot(bool bShine = true) : base(true, bShine)
		{	uvApunta = new uVector() {	Text = "Points at"};
				uvApunta.ValueChanged += Ctl_Changed; InsertControl(1, uvApunta);
			uslFoco = new uSlider() {	Text= "Focus", Maximum = 200};
				uslFoco.ValueChanged += Ctl_Changed; InsertControl(2, uslFoco);
			uslCono = new uSlider() {	Text= "Cone angle", Maximum = 90};
				uslCono.ValueChanged += Ctl_Changed; InsertControl(3, uslCono);
		}
		public Vector Apunta
		{	get												{	return uvApunta.Value;}
		}
		public float Foco
		{	get												{	return uslFoco.Value;}
		}
		public float Cono
		{	get												{	return uslCono.Value;}
		}
		public override void IniEfecto()
		{	Point pt = (wMain.VistaActual?.Tam).GetValueOrDefault(Point.Identity); uvApunta.Value = new Vector(pt.X / 2, pt.Y / 2, 0);
			uslFoco.Value = 10; uslCono.Value = 10; base.IniEfecto();
		}
	}
	private class uLuzDist : uEfLuz
	{	uSlider uslAcimut, uslEleva;
		public uLuzDist(bool bShine = true) : base(false, bShine)
		{	uslAcimut = new uSlider() {	Text= "Azimuth", Maximum = 360};
				uslAcimut.ValueChanged += Ctl_Changed; InsertControl(0, uslAcimut);
			uslEleva = new uSlider() {	Text= "Elevation", Maximum = 360};
				uslEleva.ValueChanged += Ctl_Changed; InsertControl(1, uslEleva);
		}
		public float Acimut
		{	get												{	return uslAcimut.Value;}
		}
		public float Eleva
		{	get												{	return uslEleva.Value;}
		}
		public override void IniEfecto()					{	uslAcimut.Value = 0; uslEleva.Value = 0; base.IniEfecto();}
	}
	private delegate cEffect dCreaEf();
// ** Cpos
	cEffect m_efEfActual;
	cPrimObj3D m_p3dActual;
	uSlider uslHue, uslSat; cCbo ccbColorPresets; uMatrix umaColor, umaGammaT; uTransform umaDiscreT, umaTableT;
		uSlider uslTurbuFrec, uslTurbuOctaves; cNumericTextBox ntTurbuSeed; cCheckBox chkTurbuNoise, chkTurbuStitch;
	uSlider uslBrilloWX, uslBrilloWY, uslBrilloBX, uslBrilloBY;
	uSlider uslGauss, uslDirBlurDesv, uslDirBlurAng, uslConvolveDivi, uslConvolveBias; cCbo ccbConvolvePresets; uMatrix umaConvolve;
		uPoint uptConvolveDesp; cCheckBox chkConvolveAlfa, chkThickThick; uSlider uslThickKAncho, uslThickKAlto, uslDisplaceEscala;
		uEfOrig uoDisplaceOrig; cCbo ccbDisplaceX; cComboBox cboDisplaceY;
		cButton btnDft;
	uSlider uslShadowDesv; uPoint uptShadowOffset; cColorButton clbShadowColor;
		uEfLuz leMake3D, lePointSpec, lePointDiff; uLuzSpot lesSpotSpec, lesSpotDiff; uLuzDist ledDistSpec, ledDistDiff;
		uSlider uslMake3DThickness;
	uSlider uslRot, uslWaveDesp, uslWaveSkewX, uslWaveSkewY;
		cCheckBox chkFlipH, chkFlipV;
		cCbo ccbVertPresets; uMatrix umaVert;
		cCbo ccbTileModoX, ccbTileModoY;
		uSlider uslRippleFrec, uslRippleFase, uslRippleAmp, uslRippleSpread, uslRippleLim;
			uPoint uptRippleCenter;
		uSlider uslBendAng, uslBendDist, uslBendRadio;
		uSlider uslWhirlRadio, uslWhirlSpin; uPoint uptWhirlCenter;
		uSlider uslBubbleRadio, uslBubbleFlatten, uslBubbleFalloff, uslBubbleLuz; uPoint uptBubbleCenter;
	uEfOrig uoBlendOrig, uoComposiOrig, uoAritOrig; cCbo ccbBlendModo, ccbComposiModo;
		uSlider uslAritDestxSource, uslAritDest, uslAritSource, uslAritOffset;
		cColorButton clbColorKeyColor; cToolButton tbbColorKeyMuestra; uEfOrig uoColorKeyFondo; uSlider uslColorKeyTolera;
	u3DModelControl u3dObj;
// ** Ctor/dtor
public kEfectos()
{	cTabControl tc; cTabControl.cTab tab; cScrollableControl scCli; cStackPanel spnCli;
	uPropertyGroup grp; uPropertySubgroup sgr; cContainer cnt; cToolButton tbt; int i, j;

	Width = 390; IsChildForm = false; Text = "Effects"; HideOnClose = true; Dock = eDirection.Left;
	tc = new cTabControl() {	Margins = new Rect(0)};
	tc.LayoutSuspended = true; tc.Header.LayoutSuspended = true;
	// ** Color
	tab = tc.Tabs.Add("Color");
	scCli = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; scCli.ClientArea.SuspendLayout();
	spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup() {	Text = "Hue / Saturation"};
		uslHue = new uSlider() {	Text = "Hue", IsPercent = true};
			uslHue.ValueChanged += uslHueSat_ValueChanged; grp.AddControl(uslHue);
		uslSat = new uSlider() {	Text = "Saturation", IsPercent = true};
			uslSat.ValueChanged += uslHueSat_ValueChanged; grp.AddControl(uslSat);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Color matrix"};
		ccbColorPresets = new cCbo()
				{	Items = new string[] {	"Default", "Red channel", "Green channel", "Blue channel", "Yellow channel"
						, "Magenta channel", "Cyan channel", "Red", "Green", "Blue", "Yellow", "Magenta", "Cyan", "Black", "White"
						, "Gray scale", "Invert", "Swap red-green", "Swap red-blue", "Swap green-blue", "Image mask (luminance to alpha)"
						, "Invert alpha"}
					, SelectedIndex = 0, Text = "Presets"
				};
			ccbColorPresets.SelectionChanged += cboColorPresets_SelectionChanged; grp.AddControl(ccbColorPresets);
		umaColor = new uMatrix() {	Height = 155};
			umaColor.Changed += umaColor_Changed; grp.AddControl(umaColor);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Gamma transfer"};
		umaGammaT = new uMatrix() {	Height = 135};
			umaGammaT.Changed += umaGammaT_Changed; grp.AddControl(umaGammaT);
		spnCli.AddControl(grp);
	umaDiscreT = new uTransform() {	Text = "Discrete transfer"}; umaDiscreT.Changed += DiscreT_Changed;
		spnCli.AddControl(umaDiscreT);
	umaTableT = new uTransform() {	Text = "Table transfer"}; umaTableT.Changed += TableT_Changed;
		spnCli.AddControl(umaTableT);
	grp = new uPropertyGroup() {	Text = "Turbulence"};
		uslTurbuFrec = new uSlider() {	Text = "Base frequency", Maximum = 100};
			uslTurbuFrec.ValueChanged += Turbu_Changed; grp.AddControl(uslTurbuFrec);
		uslTurbuOctaves = new uSlider() {	Text = "Octaves", Maximum = 5};
			uslTurbuOctaves.ValueChanged += Turbu_Changed; grp.AddControl(uslTurbuOctaves);
		sgr = new uPropertySubgroup() {	Text = "Seed"};
			ntTurbuSeed = new cNumericTextBox() {	Type = eNumberType.Zero | eNumberType.Positive, Width = 87};
				ntTurbuSeed.ValueChanged += Turbu_Changed; sgr.AddControl(ntTurbuSeed);
			grp.AddControl(sgr);
		cnt = new cContainer() {	AutoSize = eAutoSize.Height};
			chkTurbuNoise = new cCheckBox() {	Text = "Noise"};
				chkTurbuNoise.CheckStateChanged += Turbu_Changed; cnt.AddControl(chkTurbuNoise);
			chkTurbuStitch = new cCheckBox() {	Text = "Stitchable", LeftMargin = 110};
				chkTurbuStitch.CheckStateChanged += Turbu_Changed; cnt.AddControl(chkTurbuStitch);
			grp.AddControl(cnt);
		spnCli.AddControl(grp);
	scCli.ClientArea.AddControl(spnCli); scCli.ClientArea.LayoutSuspended = false; tab.Content = scCli;
	// ** Foto
	tab = tc.Tabs.Add("Photo");
	scCli = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; scCli.ClientArea.SuspendLayout();
	spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup() {	Text = "Brightness"};
		uslBrilloWX = new uSlider() {	Text = "White point X", IsPercent = true};
			uslBrilloWX.ValueChanged += uslBrillo_ValueChanged; grp.AddControl(uslBrilloWX);
		uslBrilloWY = new uSlider() {	Text = "White point Y", IsPercent = true};
			uslBrilloWY.ValueChanged += uslBrillo_ValueChanged; grp.AddControl(uslBrilloWY);
		uslBrilloBX = new uSlider() {	Text = "Black point X", IsPercent = true};
			uslBrilloBX.ValueChanged += uslBrillo_ValueChanged; grp.AddControl(uslBrilloBX);
		uslBrilloBY = new uSlider() {	Text = "Black point Y", IsPercent = true};
			uslBrilloBY.ValueChanged += uslBrillo_ValueChanged; grp.AddControl(uslBrilloBY);
		spnCli.AddControl(grp);
	scCli.ClientArea.AddControl(spnCli); scCli.ClientArea.LayoutSuspended = false; tab.Content = scCli;
	// ** Filtro
	tab = tc.Tabs.Add("Filter");
	scCli = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; scCli.ClientArea.SuspendLayout();
	spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup() {	Text = "Gaussian blur"};
		uslGauss = new uSlider() {	Text = "Deviation", IsPercent = true};
			uslGauss.ValueChanged += uslGauss_ValueChanged; grp.AddControl(uslGauss);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Directional blur"};
		uslDirBlurDesv = new uSlider() {	Text = "Deviation", IsPercent = true};
			uslDirBlurDesv.ValueChanged += uslDirBlur_ValueChanged; grp.AddControl(uslDirBlurDesv);
		uslDirBlurAng = new uSlider() {	Text = "Angle", IsPercent = true};
			uslDirBlurAng.ValueChanged += uslDirBlur_ValueChanged; grp.AddControl(uslDirBlurAng);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Convolve matrix"};
		ccbConvolvePresets = new cCbo() {	Text = "Presets"
				, Items = new string[] {	"Default", "Blur", "Sharpen", "Edge detection", "Emboss"}, SelectedIndex = 0};
			ccbConvolvePresets.SelectionChanged += cboConvolvePresets_SelectionChanged; grp.AddControl(ccbConvolvePresets);
		umaConvolve = new uMatrix() {	Height = 115}; umaConvolve.Changed += Convolve_Changed;
			umaConvolve.Open += ConvolveAbrir_Click; umaConvolve.Save += ConvolveGrabar_Click; grp.AddControl(umaConvolve);
		uptConvolveDesp = new uPoint() {	Text = "Offset"};
			uptConvolveDesp.ValueChanged += Convolve_Changed; grp.AddControl(uptConvolveDesp);
		uslConvolveDivi = new uSlider() {	Text = "Divisor", Minimum = -20, Maximum = 20, SmallChange = 0.5f};
			uslConvolveDivi.ValueChanged += Convolve_Changed; grp.AddControl(uslConvolveDivi);
		uslConvolveBias = new uSlider() {	Text = "Bias", Minimum = -2, Maximum = 2, SmallChange = 0.02f};
			uslConvolveBias.ValueChanged += Convolve_Changed; grp.AddControl(uslConvolveBias);
		chkConvolveAlfa = new cCheckBox() {	Text = "Preserve alpha", LeftMargin = 110};
			chkConvolveAlfa.CheckStateChanged += Convolve_Changed; grp.AddControl(chkConvolveAlfa);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Thickness"};
		chkThickThick = new cCheckBox() {	Text = "Thick"};
			chkThickThick.CheckStateChanged += Thick_Changed; grp.AddControl(chkThickThick);
		uslThickKAncho = new uSlider() {	Text = "Width", Minimum = 1};
			uslThickKAncho.ValueChanged += Thick_Changed; grp.AddControl(uslThickKAncho);
		uslThickKAlto = new uSlider() {	Text = "Height", Minimum = 1};
			uslThickKAlto.ValueChanged += Thick_Changed; grp.AddControl(uslThickKAlto);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Displacement"};
		uoDisplaceOrig = new uEfOrig() {	Text = "Map"};
			uoDisplaceOrig.Changed += Displace_Changed; grp.AddControl(uoDisplaceOrig);
		uslDisplaceEscala = new uSlider() {	Text = "Scale", Maximum = 1000};
			uslDisplaceEscala.ValueChanged += Displace_Changed; grp.AddControl(uslDisplaceEscala);
		ccbDisplaceX = new cCbo(82) {	Text = "Channels", Items = new string[] {	"R", "G", "B", "A"}};
			ccbDisplaceX.SelectionChanged += Displace_Changed;
			cboDisplaceY = new cComboBox() {	Width = 82, Data = ccbDisplaceX.Items};
				cboDisplaceY.SelectionChanged += Displace_Changed; ccbDisplaceX.AddControl(cboDisplaceY);
			grp.AddControl(ccbDisplaceX);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Discrete Fourier Transform"};
		btnDft = new cButton() {	Text = "Apply", LeftMargin = 110};
			btnDft.Click += Dft_Changed; grp.AddControl(btnDft, false);
		spnCli.AddControl(grp);
	scCli.ClientArea.AddControl(spnCli); scCli.ClientArea.LayoutSuspended = false; tab.Content = scCli;
	// ** Iluminación
	tab = tc.Tabs.Add("Lighting");
	scCli = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; scCli.ClientArea.SuspendLayout();
	spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup() {	Text = "Shadow"};
		uslShadowDesv = new uSlider() {	Text = "Deviation", IsPercent = true};
			uslShadowDesv.ValueChanged += Shadow_Changed; grp.AddControl(uslShadowDesv);
		uptShadowOffset = new uPoint() {	Text = "Offset"};
			uptShadowOffset.ValueChanged += Shadow_Changed; grp.AddControl(uptShadowOffset);
		sgr = new uPropertySubgroup() {	Text = "Color"};
			clbShadowColor = new cColorButton();
				clbShadowColor.ColorChanged += Shadow_Changed; sgr.AddControl(clbShadowColor);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	leMake3D = new uEfLuz(true, true, true) {	Text = "Relief"};
			leMake3D.Changed += Make3D_Changed;
		uslMake3DThickness = new uSlider() {	Text = "Thickness", Maximum = 20};
			uslMake3DThickness.ValueChanged += Make3D_Changed; leMake3D.InsertControl(0, uslMake3DThickness);
		spnCli.AddControl(leMake3D);
	lePointSpec = new uEfLuz() {	Text = "Point specular"};
		lePointSpec.Changed += PointSpec_Changed; spnCli.AddControl(lePointSpec);
	lePointDiff = new uEfLuz(true, false) {	Text = "Point diffuse"};
		lePointDiff.Changed += PointDiff_Changed; spnCli.AddControl(lePointDiff);
	lesSpotSpec = new uLuzSpot() {	Text = "Spot specular"};
		lesSpotSpec.Changed += SpotSpec_Changed; spnCli.AddControl(lesSpotSpec);
	lesSpotDiff = new uLuzSpot(false) {	Text = "Spot diffuse"};
		lesSpotDiff.Changed += SpotDiff_Changed; spnCli.AddControl(lesSpotDiff);
	ledDistSpec = new uLuzDist() {	Text = "Distant specular"};
		ledDistSpec.Changed += DistSpec_Changed; spnCli.AddControl(ledDistSpec);
	ledDistDiff = new uLuzDist(false) {	Text = "Distant diffuse"};
		ledDistDiff.Changed += DistDiff_Changed; spnCli.AddControl(ledDistDiff);
	scCli.ClientArea.AddControl(spnCli); scCli.ClientArea.LayoutSuspended = false; tab.Content = scCli;
	// ** Transformación
	tab = tc.Tabs.Add("Transform");
	scCli = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; scCli.ClientArea.SuspendLayout();
	spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup() {	Text = "Rotation"};
		uslRot = new uSlider() {	Text = "Value", Maximum = 360};
			uslRot.ValueChanged += uslRot_ValueChanged; grp.AddControl(uslRot);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Flip"};
		cnt = new cContainer() {	AutoSize = eAutoSize.Height};
			chkFlipH = new cCheckBox() {	Text = "Horizontal"};
				chkFlipH.CheckStateChanged += Flip_CheckStateChanged; cnt.AddControl(chkFlipH);
			chkFlipV = new cCheckBox() {	Text = "Vertical", LeftMargin = 150};
				chkFlipV.CheckStateChanged += Flip_CheckStateChanged; cnt.AddControl(chkFlipV);
			grp.AddControl(cnt);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Coordinate matrix"};
		ccbVertPresets = new cCbo() {	Items = new string[] {	"Default", "Stretch", "Shrink", "Offset"
					, "Skew X", "Skew Y", "Yaw", "Pitch", "Flip horizontal", "Flip vertical"}
				, SelectedIndex = 0, Text = "Presets"};
			ccbVertPresets.SelectionChanged += cboVertPresets_SelectionChanged; grp.AddControl(ccbVertPresets);
		umaVert = new uMatrix() {	Height = 135}; umaVert.Changed += umaVert_Changed;
			grp.AddControl(umaVert);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Tile"};
		ccbTileModoX = new cCbo() {	Text = "Mode X", Items = new string[] {	"Clamp", "Wrap", "Mirror"}};
			ccbTileModoX.SelectionChanged += Tile_Changed; grp.AddControl(ccbTileModoX);
		ccbTileModoY = new cCbo() {	Text = "Mode Y", Items = ccbTileModoX.Items};
			ccbTileModoY.SelectionChanged += Tile_Changed; grp.AddControl(ccbTileModoY);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Wave"};
		uslWaveDesp = new uSlider() {	Text = "Offset"};
			uslWaveDesp.ValueChanged += Wave_Changed; grp.AddControl(uslWaveDesp);
		uslWaveSkewX = new uSlider() {	Text = "Skew X", Minimum = -5, Maximum = 5};
			uslWaveSkewX.ValueChanged += Wave_Changed; grp.AddControl(uslWaveSkewX);
		uslWaveSkewY = new uSlider() {	Text = "Skew Y", Minimum = -5, Maximum = 5};
			uslWaveSkewY.ValueChanged += Wave_Changed; grp.AddControl(uslWaveSkewY);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Ripple"};
		uslRippleFrec = new uSlider() {	Text = "Frequency", Maximum = 100};
			uslRippleFrec.ValueChanged += Ripple_Changed; grp.AddControl(uslRippleFrec);
		uslRippleFase = new uSlider() {	Text = "Phase", Minimum = -10, Maximum = 10};
			uslRippleFase.ValueChanged += Ripple_Changed; grp.AddControl(uslRippleFase);
		uslRippleAmp = new uSlider() {	Text = "Amplitude", Minimum = 0.0001f, Maximum = 1000};
			uslRippleAmp.ValueChanged += Ripple_Changed; grp.AddControl(uslRippleAmp);
		uslRippleSpread = new uSlider() {	Text = "Spread", Minimum = 0.0001f, Maximum = 50};
			uslRippleSpread.ValueChanged += Ripple_Changed; grp.AddControl(uslRippleSpread);
		uptRippleCenter = new uPoint() {	Text = "Center"};
			uptRippleCenter.ValueChanged += Ripple_Changed; grp.AddControl(uptRippleCenter);
		uslRippleLim = new uSlider() {	Text = "Limit", Minimum = 0.0001f, Maximum = 2};
			uslRippleLim.ValueChanged += Ripple_Changed; grp.AddControl(uslRippleLim);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Bend"};
		uslBendAng = new uSlider() {	Text = "Angle", Maximum = 90};
			uslBendAng.ValueChanged += Bend_Changed; grp.AddControl(uslBendAng);
		uslBendDist = new uSlider() {	Text = "Distance", Minimum = -1, Maximum = 1};
			uslBendDist.ValueChanged += Bend_Changed; grp.AddControl(uslBendDist);
		uslBendRadio = new uSlider() {	Text = "Radius", IsPercent = true};
			uslBendRadio.ValueChanged += Bend_Changed; grp.AddControl(uslBendRadio);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Whirl"};
		uptWhirlCenter = new uPoint() {	Text = "Center"};
			uptWhirlCenter.ValueChanged += Whirl_Changed; grp.AddControl(uptWhirlCenter);
		uslWhirlRadio = new uSlider() {	Text = "Radius", Minimum = 0.0001f, Maximum = 4};
			uslWhirlRadio.ValueChanged += Whirl_Changed; grp.AddControl(uslWhirlRadio);
		uslWhirlSpin = new uSlider() {	Text = "Spin", Minimum = -3, Maximum = 3};
			uslWhirlSpin.ValueChanged += Whirl_Changed; grp.AddControl(uslWhirlSpin);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Bubble"};
		uptBubbleCenter = new uPoint() {	Text = "Center"};
			uptBubbleCenter.ValueChanged += Bubble_Changed; grp.AddControl(uptBubbleCenter);
		uslBubbleRadio = new uSlider() {	Text = "Radius", Minimum = 0.0001f, Maximum = 2};
			uslBubbleRadio.ValueChanged += Bubble_Changed; grp.AddControl(uslBubbleRadio);
		uslBubbleFlatten = new uSlider() {	Text = "Flatten", IsPercent = true};
			uslBubbleFlatten.ValueChanged += Bubble_Changed; grp.AddControl(uslBubbleFlatten);
		uslBubbleFalloff = new uSlider() {	Text = "Falloff", Maximum = 50};
			uslBubbleFalloff.ValueChanged += Bubble_Changed; grp.AddControl(uslBubbleFalloff);
		uslBubbleLuz = new uSlider() {	Text = "Light", Minimum = -1, Maximum = 1};
			uslBubbleLuz.ValueChanged += Bubble_Changed; grp.AddControl(uslBubbleLuz);
		spnCli.AddControl(grp);
	scCli.ClientArea.AddControl(spnCli); scCli.ClientArea.LayoutSuspended = false; tab.Content = scCli;
	// ** Composición
	tab = tc.Tabs.Add("Composition");
	scCli = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; scCli.ClientArea.SuspendLayout();
	spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup() {	Text = "Blend"};
		uoBlendOrig = new uEfOrig();
			uoBlendOrig.Changed += Blend_Changed; grp.AddControl(uoBlendOrig);
		ccbBlendModo = new cCbo() {	Text = "Mode"
				, Items = new string[] {	"Multiply", "Screen", "Darken", "Lighten", "Dissolve", "ColorBurn", "LinearBurn"
					, "DarkerColor", "LighterColor", "ColorDodge", "LinearDodge", "Overlay", "SoftLight", "HardLight", "VividLight"
					, "LinearLight", "PinLight", "HardMix", "Difference", "Exclusion", "Hue", "Saturation", "Color", "Luminosity"
					, "Subtract", "Division"}};
			ccbBlendModo.SelectionChanged += Blend_Changed; grp.AddControl(ccbBlendModo);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Composite"};
		uoComposiOrig = new uEfOrig();
			uoComposiOrig.Changed += Composi_Changed; grp.AddControl(uoComposiOrig);
		ccbComposiModo = new cCbo() {	Text = "Mode"
				, Items = new string[] {	"Default", "DestinationOver", "SourceIn", "DestinationIn", "SourceOut"
					, "DestinationOut", "SourceAtop", "DestinationAtop", "Xor", "Plus", "SourceCopy", "BoundedSourceCopy", "MaskInvert"}};
			ccbComposiModo.SelectionChanged += Composi_Changed; grp.AddControl(ccbComposiModo);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Arithmetic"};
		uoAritOrig = new uEfOrig();
			uoAritOrig.Changed += Arit_Changed; grp.AddControl(uoAritOrig);
		uslAritDestxSource = new uSlider() {	Text = "Source x Dest", IsPercent = true};
			uslAritDestxSource.ValueChanged += Arit_Changed; grp.AddControl(uslAritDestxSource);
		uslAritDest = new uSlider() {	Text = "Destination", IsPercent = true};
			uslAritDest.ValueChanged += Arit_Changed; grp.AddControl(uslAritDest);
		uslAritSource = new uSlider() {	Text = "Source", IsPercent = true};
			uslAritSource.ValueChanged += Arit_Changed; grp.AddControl(uslAritSource);
		uslAritOffset = new uSlider() {	Text = "Offset", IsPercent = true};
			uslAritOffset.ValueChanged += Arit_Changed; grp.AddControl(uslAritOffset);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup() {	Text = "Color key"};
		sgr = new uPropertySubgroup() {	Text = "Color"};
			tbbColorKeyMuestra = new cToolButton() {	Bitmap = mRes.BmpMuestra, ToolTip = "Pick color"};
				tbbColorKeyMuestra.Click += tbbColorKeyMuestra_Click; tbbColorKeyMuestra.MouseDown += tbbColorKeyMuestra_MouseDown;
				 sgr.AddControl(tbbColorKeyMuestra);
			clbColorKeyColor = new cColorButton();
				clbColorKeyColor.ColorChanged += ColorKey_Changed; sgr.AddControl(clbColorKeyColor);
			grp.AddControl(sgr);
		uoColorKeyFondo = new uEfOrig() {	Text = "Background"};
			uoColorKeyFondo.Changed += ColorKey_Changed; grp.AddControl(uoColorKeyFondo);
		uslColorKeyTolera = new uSlider() {	Text = "Tolerance", Value = 50};
			uslColorKeyTolera.ValueChanged += ColorKey_Changed; grp.AddControl(uslColorKeyTolera);
		spnCli.AddControl(grp);
	scCli.ClientArea.AddControl(spnCli); scCli.ClientArea.LayoutSuspended = false; tab.Content = scCli;
	// ** Obj 3D
	tab = tc.Tabs.Add("3D object");
	scCli = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical}; scCli.ClientArea.SuspendLayout();
	spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup() {	Text = "Model"};
		sgr = new uPropertySubgroup() {	Text = "Add"};
			tbt = new cToolButton() {	Bitmap = mRes.BmpAbrir, ToolTip = "Open"};
				tbt.Click += tbt3DObjAbrir_Click; sgr.AddControl(tbt);
			tbt = new cToolButton() {	Bitmap = mRes.BmpPegar, ToolTip = "Paste"};
				tbt.Click += tbt3DObjPegar_Click; sgr.AddControl(tbt);
			grp.AddControl(sgr);
		u3dObj = new u3DModelControl() {	ApplyMeshToTree = true}; u3dObj.ModelChanged += u3dObj_Changed;
			u3dObj.ModelInvalidated += u3dObj_ModelInvalidated; grp.AddControl(u3dObj);
		spnCli.AddControl(grp);
	scCli.ClientArea.AddControl(spnCli); scCli.ClientArea.LayoutSuspended = false; tab.Content = scCli;
	tc.LayoutSuspended = false; tc.Header.LayoutSuspended = false;
	AddControl(tc);
	// ** Cfg
	umaColor.LayoutSuspended = true;
		umaColor.Initialize(new string[] {	"Red", "Green", "Blue", "Alpha"}, 5);
		for (j = 0; j < 5; j++)
		{	for (i = 0; i < 4; i++)	umaColor.SetRanges(j, i, -100, 100);	// Usar %
		}
	umaColor.LayoutSuspended = false;
	umaGammaT.LayoutSuspended = true;
		umaGammaT.Initialize(new string[] {	"Red", "Green", "Blue", "Alpha"}, 4);///3);///cabs de fila: Amplitude, Exponent, Offset
		for (j = 0; j < 3; j++)
		{	for (i = 0; i < 4; i++)	umaGammaT.SetRanges(j, i, -100, 100);	// Usar %
		}
	umaGammaT.LayoutSuspended = false;
	umaConvolve.LayoutSuspended = true;
		umaConvolve.Initialize(new string[] {	"1", "2", "3"}, 3);
		for (j = 0; j < 3; j++)
		{	for (i = 0; i < 3; i++) umaConvolve.SetRanges(j, i, -20, 20, 0.2f);
		}
	umaConvolve.LayoutSuspended = false;
	umaVert.LayoutSuspended = true;
		umaVert.Initialize(new string[] {	"X", "Y", "Z", "W"}, 4);
		for (j = 0; j < 3; j++)												// Escalas
		{	for (i = 0; i < 3; i++)	umaVert.SetRanges(j, i, -10, 10, 0.2f);
		}
		for (j = 0; j < 3; j++)	umaVert.SetRanges(j, 3, -0.1f, 0.1f, 0.002f); // Col w.  Los desps se cfg junto con los efs
	umaVert.LayoutSuspended = false;
	r_Commands.Add(new cCommandState(eCmds.EfNuevo, NuevoEf_Execute));
}
// ** Props
public cEffect EfActual
{	get														{	return m_efEfActual;}
}
// ** Mets
public void LimpiaEfectos()									{	m_efEfActual = null; m_p3dActual = null; m_IniEfectos();}
public void Redim()											{	m_tGetEfecto<cScaleEffect>(null, () => new cScaleEffect());}
public void Estira(Point ptTamAnt, Point ptTamActual)
{	cEfEstirar est;

	est = m_tGetEfecto<cEfEstirar>(null, () => new cEfEstirar() {	TamIni = ptTamAnt});
	est.Scale = ptTamActual / est.TamIni;
}
public void Recorta(Rectangle rtRegión)
{	cAtlasEffect cre;

	cre = m_tGetEfecto<cAtlasEffect>(null, () => new cAtlasEffect() {	Region = new Rectangle(Point.Zero, rtRegión.Size)});
	cre.Region = new Rectangle(cre.Region.Location + rtRegión.Location, rtRegión.Size); // ** Acumular recorte
}
public void Rota(float fAng, bool bEsRel = false)
{	cEfRot ero;

	ero = m_tGetEfecto<cEfRot>(uslRot, () => new cEfRot());
	if (bEsRel)	ero.Ang += fAng;	else	ero.Ang = fAng;
}
public void Pinta3D(PaintArgs e)							{	m_p3dActual?.Pinta3D(e); }
public void TerminaEdición3D()
{		if (m_p3dActual == null)	return;
	m_p3dActual.TerminaEdición(); m_p3dActual = null;
	u3dObj.Model = null; wMain.VistaActual?.Invalida();
}
private void NuevoEf_Execute(cCommandState command, object args = null)	{	m_efEfActual = null;}
private void uslHueSat_ValueChanged(object sender)
{	cColorMatrixEffect sae;

	sae = m_tGetEfecto<cColorMatrixEffect>(uslHue, () => new cColorMatrixEffect());
	sae.SetHueSaturation(uslHue.Value, uslSat.Value);
}
private void cboColorPresets_SelectionChanged(object sender)
{	string s;

	switch (ccbColorPresets.SelectedIndex)
	{	case 1:	s = "1; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 0; 0; 0"; break; // Red channel
		case 2: s = "0; 0; 0; 0;  0; 1; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 0; 0; 0"; break; // Green channel
		case 3: s = "0; 0; 0; 0;  0; 0; 1; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 0; 0; 0"; break; // Blue channel
		case 4: s = "1; 0; 0; 0;  0; 1; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 0; 0; 0"; break; // Yellow channel
		case 5: s = "1; 0; 0; 0;  0; 0; 0; 0;  0; 0; 1; 0;  0; 0; 0; 1;  0; 0; 0; 0"; break; // Magenta channel
		case 6: s = "0; 0; 0; 0;  0; 1; 0; 0;  0; 0; 1; 0;  0; 0; 0; 1;  0; 0; 0; 0"; break; // Cyan channel
		case 7: s = "0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  1; 0; 0; 0"; break; // Red
		case 8: s = "0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 1; 0; 0"; break; // Green
		case 9:	s = "0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 0; 1; 0"; break; // Blue
		case 10:	s = "0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  1; 1; 0; 0"; break; // Yellow
		case 11:	s = "0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  1; 0; 1; 0"; break; // Magenta
		case 12:	s = "0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 1; 1; 0"; break; // Cyan
		case 13:	s = "0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  0; 0; 0; 0"; break; // Black
		case 14:	s = "0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 0;  0; 0; 0; 1;  1; 1; 1; 0"; break; // White
		case 15:	s = "0.213; 0.213; 0.213; 0;  0.715; 0.715; 0.715; 0;  0.072; 0.072; 0.072; 0;  0; 0; 0; 1;  0; 0; 0; 0"; break; // Gray scale
		case 16:	s = "-1; 0; 0; 0;  0; -1; 0; 0;  0; 0; -1; 0;  0; 0; 0; 1;  1; 1; 1; 0"; break; // Invert
		case 17:	s = "0; 1; 0; 0;  1; 0; 0; 0;  0; 0; 1; 0;  0; 0; 0; 1;  0; 0; 0; 0"; break; // Swap red-green
		case 18:	s = "0; 0; 1; 0;  0; 1; 0; 0;  1; 0; 0; 0;  0; 0; 0; 1;  0; 0; 0; 0"; break; // Swap red-blue
		case 19:	s = "1; 0; 0; 0;  0; 0; 1; 0;  0; 1; 0; 0;  0; 0; 0; 1;  0; 0; 0; 0"; break; // Swap green-blue
		case 20:	s = "0; 0; 0; 0.2125;  0; 0; 0; 0.7154;  0; 0; 0; 0.0721;  0; 0; 0; 0;  0; 0; 0; 0"; break; // Image mask (Luminance to alpha)
		case 21:	s = "1; 0; 0; 0;  0; 1; 0; 0;  0; 0; 1; 0;  0; 0; 0; -1;  0; 0; 0; 1"; break; // Invert alpha
		default:	s = "1; 0; 0; 0;  0; 1; 0; 0;  0; 0; 1; 0;  0; 0; 0; 1;  0; 0; 0; 0"; break; // Default
	}
	umaColor.Matrix5x4 = new Matrix5x4(s) * 100; umaColor_Changed(null);
}
private void umaColor_Changed(object sender)
{	cColorMatrixEffect cme;

	cme = m_tGetEfecto<cColorMatrixEffect>(umaColor, () => new cColorMatrixEffect());
	cme.Matrix = umaColor.Matrix5x4 * 0.01f;								// Conv a % (puede ser nega)
}
private void umaGammaT_Changed(object sender)
{	cGammaTransferEffect gte; Matrix4x4 m44;

	gte = m_tGetEfecto<cGammaTransferEffect>(umaGammaT, () => new cGammaTransferEffect());
	m44 = umaGammaT.Matrix4x4 * 0.01f;										// Conv a % (puede ser nega)
	gte.RedAmplitude = m44._11; gte.RedExponent = m44._21; gte.RedOffset = m44._31;
	gte.GreenAmplitude = m44._12; gte.GreenExponent = m44._22; gte.GreenOffset = m44._32;
	gte.BlueAmplitude = m44._13; gte.BlueExponent = m44._23; gte.BlueOffset = m44._33;
	gte.AlphaAmplitude = m44._14; gte.AlphaExponent = m44._24; gte.AlphaOffset = m44._34;
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
	te.Bounds = new Rectangle(Point.Zero, (wMain.VistaActual?.Tam).GetValueOrDefault());
	te.BaseFrequency = new Point(uslTurbuFrec.Value, uslTurbuFrec.Value) / 500;
	te.Octaves = (int)uslTurbuOctaves.Value; te.Seed = ntTurbuSeed.IntValue;
	te.Noise = chkTurbuNoise.Checked; te.Stitchable = chkTurbuStitch.Checked;
}
private void uslBrillo_ValueChanged(object sender)
{	cBrightnessEffect bef;

	bef = m_tGetEfecto<cBrightnessEffect>(uslBrilloWX, () => new cBrightnessEffect());
	bef.WhitePoint = new Point(uslBrilloWX.Value, uslBrilloWY.Value); bef.BlackPoint = new Point(uslBrilloBX.Value, uslBrilloBY.Value);
}
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
	umaConvolve.Matrix3x3 = new Matrix3x3(s); Convolve_Changed(null);
}
private void Convolve_Changed(object sender)
{	cConvolveMatrixEffect coe; Matrix3x3 m33;

	coe = m_tGetEfecto<cConvolveMatrixEffect>(umaConvolve, () => new cConvolveMatrixEffect());
	m33 = umaConvolve.Matrix3x3;
	coe.Matrix = new float[] { m33._11, m33._12, m33._13, m33._21, m33._22, m33._23, m33._31, m33._32, m33._33};
	coe.Offset = uptConvolveDesp.Value; coe.Divisor = uslConvolveDivi.Value;
	coe.Bias = uslConvolveBias.Value; coe.PreserveAlpha = chkConvolveAlfa.Checked == true;
}
private void ConvolveAbrir_Click(object sender, out bool handled)
{	string s; System.Xml.XmlDocument xd = null; System.Xml.XmlElement xe;

	handled = true;
	try
	{	s = mDialog.ShowOpenFile("Convolve matrix|*.convolvemx|All files|*.*", new System.Guid("{7BADF31B-94E4-4216-A9D2-CF9F5EC94E2C}"));
			if (s == null) return;											// Cancelado: salir
		xd = new System.Xml.XmlDocument(); xd.Load(s); xe = xd["convolvemx"];
		umaConvolve.Matrix3x3 = new Matrix3x3(xe.Attributes["value"].Value);
		uptConvolveDesp.Value = new Point(xe.Attributes["offset"].Value);
		uslConvolveDivi.Value = xe.Attributes["divisor"].Value.ToFloat();
		uslConvolveBias.Value = xe.Attributes["bias"].Value.ToFloat();
	} catch (System.Exception ex)
	{	mDialog.MsgBoxExclamation(ex.Message, "Error");
	}
}
private void ConvolveGrabar_Click(object sender, out bool handled)
{	string s; System.Xml.XmlWriter xw = null;

	handled = true;
	try
	{	s = mDialog.ShowSaveFile("Convolve matrix|*.convolvemx", new System.Guid("{7BADF31B-94E4-4216-A9D2-CF9F5EC94E2C}"));
			if (s == null) return;											// Cancelado: salir
		xw = System.Xml.XmlWriter.Create(s, new System.Xml.XmlWriterSettings {	Indent = true, IndentChars = "\t"});
		xw.WriteStartElement("convolvemx");
			xw.WriteAttributeString("value", umaConvolve.Matrix3x3.ConvertToString());
			xw.WriteAttributeString("offset", uptConvolveDesp.Value.ConvertToString());
			xw.WriteAttributeString("divisor", uslConvolveDivi.Value.ToStringInvariant());
			xw.WriteAttributeString("bias", uslConvolveBias.Value.ToStringInvariant());
	} catch (System.Exception ex)
	{	mDialog.MsgBoxExclamation(ex.Message, "Error");
	} finally
	{	if (xw != null)	xw.Dispose();
	}
}
private void Thick_Changed(object sender)
{	cThicknessEffect tie;

	tie = m_tGetEfecto<cThicknessEffect>(chkThickThick, () => new cThicknessEffect());
	tie.Thick = chkThickThick.Checked;
	tie.KernelWidth = (int)uslThickKAncho.Value; tie.KernelHeight = (int)uslThickKAlto.Value;
}
private void Displace_Changed(object sender)//¿pinta el cmdls desplazado
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
{	Matrix4x4 mat; Point pt = (wMain.VistaActual?.Tam).GetValueOrDefault();

	switch (ccbVertPresets.SelectedIndex)
	{	case 1:	mat = new Matrix4x4("2; 0; 0; 0;  0; 2; 0; 0;  0; 0; 1; 0;  0; 0; 0; 1"); break; // Stretch
		case 2: mat = new Matrix4x4("0.5; 0; 0; 0;  0; 0.5; 0; 0;  0; 0; 1; 0;  0; 0; 0; 1"); break; // Shrink
		case 3: mat = Matrix4x4.Identity; mat._41 = pt.X * 0.1f; mat._42 = pt.Y * 0.1f; break; // Offset
		case 4: mat = Matrix4x4.Identity; mat._21 = 1; mat._41 = -pt.X * 0.5f; break; // SkewX
		case 5: mat = Matrix4x4.Identity; mat._12 = 1; mat._42 = -pt.Y * 0.5f; break; // SkewY
		case 6: mat = new Matrix4x4("1; 0; 0; 0.005;  0; 1; 0; 0;  0; 0; 1; 0;  0; 0; 0; 1"); break; // Yaw
		case 7:	mat = new Matrix4x4("1; 0; 0; 0;  0; 1; 0; 0.02;  0; 0; 1; 0;  0; 0; 0; 1"); break; // Pitch
		case 8:	mat = Matrix4x4.Identity; mat._11 = -1; mat._41 = pt.X; break; // Flip horizontal
		case 9:	mat = Matrix4x4.Identity; mat._22 = -1; mat._42 = pt.Y; break; // Flip vertical
		default:	mat = Matrix4x4.Identity; break;						// Deafult
	}
	umaVert.Matrix4x4 = mat; umaVert_Changed(null);
}
private void umaVert_Changed(object sender)
{	c3DTransformEffect _3e;

	_3e = m_tGetEfecto<c3DTransformEffect>(umaVert, () => new c3DTransformEffect());
	_3e.Matrix = umaVert.Matrix4x4;
}
private void Tile_Changed(object sender)
{	cTileEffect tle; fVista vis = wMain.VistaActual;

		if (vis == null)	return;
	tle = m_tGetEfecto<cTileEffect>(ccbTileModoX
		, () =>
		{	cTileEffect tle2;
		
			tle2 = new cTileEffect();
			vis.IniConSelEf(new Rectangle(Point.Zero, vis.Tam / 2), () => tle2.Rectangle = vis.PlaceSel);
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
	ae.DestinationxSourceFactor = uslAritDestxSource.Value; ae.DestinationFactor = uslAritDest.Value;
	ae.SourceFactor = uslAritSource.Value; ae.OffsetFactor = uslAritOffset.Value;
}
private void tbbColorKeyMuestra_Click(object sender)
{	tbbColorKeyMuestra.Captured = true; mMouse.Global = mRes.CurMuestra;
}
private void tbbColorKeyMuestra_MouseDown(object sender, ref MouseArgs e)
{	cSolidBrush sbr;

		if (mMouse.Global != mRes.CurMuestra)	return;
	tbbColorKeyMuestra.Captured = false; mMouse.Global = null;
	sbr = wMain.VistaActual?.TomaMuestra(tbbColorKeyMuestra.PointToScreen(e.Location), true);	if (sbr == null)	return;
	clbColorKeyColor.Color = sbr.Color; sbr.Dispose();
	if (uoColorKeyFondo.Value != null)	ColorKey_Changed(null);
}
private void ColorKey_Changed(object sender)
{	cColorKeyEffect cke;

		if (!uoColorKeyFondo.Valida("Set a background first", "Color key effect"))	return; // Sin fondo: salir
	cke = m_tGetEfecto<cColorKeyEffect>(uoColorKeyFondo, () => new cColorKeyEffect(), true);
	cke.Background = uoColorKeyFondo.Value; cke.Color = clbColorKeyColor.Color; cke.Tolerance = uslColorKeyTolera.Value / 1000;
}
private void tbt3DObjAbrir_Click(object sender)
{	string s;
	
	s = mDialog.ShowOpenFile("3D models|*.mdl|All files|*.*", new System.Guid("{CE83EF0E-9CB1-4342-B716-467098272A68}"));
		if (s == null) return;
	m_3DObjAgrega(new c3DModel(mMod.MainWnd, s), (int)new System.IO.FileInfo(s).Length);
}
private void tbt3DObjPegar_Click(object sender)
{	c3DModel mdl;
	
	mdl = mClipboard.Get3DModel(mMod.MainWnd);	if (mdl == null)	{	mDialog.MsgBoxExclamation("Invalid format.", "Paste"); return;}
	m_3DObjAgrega(mdl, 100000);///tam
}
private void u3dObj_Changed(object sender)					{	m_p3dActual.Vista.Changed = true;}
private void u3dObj_ModelInvalidated(object sender)			{	m_p3dActual.Vista.Invalida();}
private void m_3DObjAgrega(c3DModel mdlMdl, int iTam)
{	fVista vis = wMain.VistaActual;

		if (vis == null)	return;
try
{	m_efEfActual = null; m_IniEfectos(u3dObj);
	if (m_p3dActual == null)	{	m_p3dActual = new cPrimObj3D(vis); vis.AddPrim(m_p3dActual);}
	m_p3dActual.Mdl = mdlMdl; u3dObj.Model = mdlMdl;
	vis.RedimÍtemUndo(m_p3dActual, iTam);
} catch (System.Exception ex)
{	mDialog.MsgBoxExclamation(ex.Message, "Error");
}
	vis.Changed = true;
}
private void m_IniEfectos(cControl ctlExcep = null)
{	if (ctlExcep != uslHue)			{	uslHue.Value = 0; uslSat.Value = 1;}
	if (ctlExcep != umaColor)		{	ccbColorPresets.SelectedIndex = 0; umaColor.Matrix5x4 = Matrix5x4.Identity * 100;} // Conv a %
	if (ctlExcep != umaGammaT)
	{	umaGammaT.Matrix4x4 = new Matrix4x4(100, 100, 100, 100, 100, 100, 100, 100, 0, 0, 0, 0, 0, 0, 0, 0);
	}
	if (ctlExcep != umaDiscreT)			umaDiscreT.Reset();
	if (ctlExcep != umaTableT)			umaTableT.Reset();
	if (ctlExcep != uslTurbuFrec)
	{	uslTurbuFrec.Value = 0.01f * 500; uslTurbuOctaves.Value = 1; ntTurbuSeed.IntValue = 0;
		chkTurbuNoise.Checked = true; chkTurbuStitch.Checked = false;
	}
	if (ctlExcep != uslBrilloWX)	{	uslBrilloWX.Value = 1; uslBrilloWY.Value = 1; uslBrilloBX.Value = 0; uslBrilloBY.Value = 0;}
	if (ctlExcep != uslGauss)		{	uslGauss.Value = 0;}
	if (ctlExcep != uslDirBlurDesv)	{	uslDirBlurDesv.Value = 0; uslDirBlurAng.Value = 0;}
	if (ctlExcep != umaConvolve)
	{	ccbConvolvePresets.SelectedIndex = 0; umaConvolve.Matrix3x3 = new Matrix3x3(0, 0, 0, 0, 1, 0, 0, 0, 0);
		uptConvolveDesp.Value = Point.Zero; uslConvolveDivi.Value = 1; uslConvolveBias.Value = 0; chkConvolveAlfa.Checked = true;
	}
	if (ctlExcep != chkThickThick)	{	chkThickThick.Checked = false; uslThickKAncho.Value = 0; uslThickKAlto.Value = 0;}
	if (ctlExcep != uoDisplaceOrig)
	{	uoDisplaceOrig.Value = null; uslDisplaceEscala.Value = 0; ccbDisplaceX.SelectedIndex = 3; cboDisplaceY.SelectedIndex = 3;
	}
	if (ctlExcep != uslShadowDesv)
	{	uslShadowDesv.Value = 0; uptShadowOffset.Value = new Point(5, 5); clbShadowColor.Color = eColor.Black;
	}
	if (ctlExcep != leMake3D)		{	leMake3D.IniEfectoMake3D(); uslMake3DThickness.Value = 3;}
	if (ctlExcep != lePointSpec)		lePointSpec.IniEfecto();
	if (ctlExcep != lePointDiff)		lePointDiff.IniEfecto();
	if (ctlExcep != lesSpotSpec)		lesSpotSpec.IniEfecto();
	if (ctlExcep != lesSpotDiff)		lesSpotDiff.IniEfecto();
	if (ctlExcep != ledDistSpec)		ledDistSpec.IniEfecto();
	if (ctlExcep != ledDistDiff)		ledDistDiff.IniEfecto();
	if (ctlExcep != uslRot)				uslRot.Value = 0;
	if (ctlExcep != chkFlipV)		{	chkFlipV.Checked = false; chkFlipH.Checked = false;}
	if (ctlExcep != umaVert)
	{	float f;
	
		ccbVertPresets.SelectedIndex = 0;
		f = (wMain.VistaActual?.Tam).GetValueOrDefault(Point.Identity).GetMaxMember().X;
			for (int i = 0; i < 3; i++)	umaVert.SetRanges(3, i, -f, f, f / 50); // Desps
		umaVert.Matrix4x4 = Matrix4x4.Identity;
	}
	if (ctlExcep != ccbTileModoX)	{	ccbTileModoX.SelectedIndex = 1; ccbTileModoY.SelectedIndex = 1;}
	if (ctlExcep != uslWaveDesp)	{	uslWaveDesp.Value = 0; uslWaveSkewX.Value = 0; uslWaveSkewY.Value = 0;}
	if (ctlExcep != uslRippleFrec)
	{	uslRippleFrec.Value = 30; uslRippleFase.Value = 0; uslRippleAmp.Value = 800;
		uslRippleSpread.Value = 10; uptRippleCenter.Value = new Point(0.5f, 0.5f); uslRippleLim.Value = 0.7f;
	}
	if (ctlExcep != uslBendAng)		{	uslBendAng.Value = 45; uslBendDist.Value = 1; uslBendRadio.Value = 0.1f;}
	if (ctlExcep != uptWhirlCenter)
	{	uptWhirlCenter.Value = new Point(0.5f, 0.5f); uslWhirlRadio.Value = 1; uslWhirlSpin.Value = 0;
	}
	if (ctlExcep != uptBubbleCenter)
	{	uptBubbleCenter.Value = new Point(0.5f, 0.5f); uslBubbleRadio.Value = 0;
		uslBubbleFlatten.Value = 0.5f; uslBubbleFalloff.Value = 10; uslBubbleLuz.Value = 0;
	}
	if (ctlExcep != uoBlendOrig)	{	uoBlendOrig.Value = null; ccbBlendModo.SelectedIndex = 0;}
	if (ctlExcep != uoComposiOrig)	{	uoComposiOrig.Value = null; ccbComposiModo.SelectedIndex = 0;}
	if (ctlExcep != uoAritOrig)
	{	uoAritOrig.Value = null;
		uslAritDestxSource.Value = 0.5f; uslAritDest.Value = 0.5f; uslAritSource.Value = 0.5f; uslAritOffset.Value = 0.5f;
	}
	if (ctlExcep != uoColorKeyFondo)
	{	uoColorKeyFondo.Value = null; clbColorKeyColor.Color = eColor.Green; uslColorKeyTolera.Value = 50;
	}
	if (ctlExcep != u3dObj)				u3dObj.Model = null;
}
private T m_tGetEfecto<T>(cControl ctlExcep /*= null*/, dCreaEf ceCreaEf, bool bMantenerSelEf = false) where T : cEffect
{	fVista vis = wMain.VistaActual;

		if (vis == null)													// Sin vis: salir
		{	if (m_efEfActual?.GetType() != typeof(T))	m_efEfActual = ceCreaEf(); // No devolver null
			return (T)m_efEfActual;
		}
	if (!bMantenerSelEf || !vis.ModoSelEf) wMain.TerminaEdición();			// ** Limpiar sel si no se pide mantener
	m_p3dActual = null; m_IniEfectos(ctlExcep);								// Limpiar los demás efs
	if (m_efEfActual?.GetType() != typeof(T))								// ** El ef actual no es del tp exacto: crear ef del nuevo tp
	{	cPrimCmdLs cml;
	
		cml = new cPrimCmdLs(vis, ceCreaEf()); m_efEfActual = cml.Efecto; vis.AddPrim(cml); // Agregar ef
	}
	vis.Changed = true;
	return (T)m_efEfActual;
}
}
}