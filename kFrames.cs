using Wew.Control;
using Wew.Media;
using Wew;

namespace DirectPaint;

sealed class kFrames : cDockControl
{	readonly cListBox lbFmes;
	public readonly cList<fVista> Frames;
	private static void s_LimpiaPropsImg()
	{	mMod.Cfg.ContainerFormat = default; mMod.Cfg.Format = ePixelFormat.Bits32BGRA; mMod.Cfg.EsCubo = false;
	}
public kFrames()
{	cButton btn;

	Width = 180; Text = "Frames"; IsForm = false; HideOnClose = true; Dock = eDirection.Left;
	Frames = [];
	lbFmes = new cListBox {	Size = new Point(70, 450)};
		lbFmes.Load(Frames);
		AddControl(lbFmes);
	btn = new cButton {	Text = "New", LocationMargin = new Point(75, 0)};
		btn.Click += New_Click;
		AddControl(btn);
	btn = new cButton {	Text = "Open", LocationMargin = new Point(75, 30)};
		btn.Click += Open_Click;
		AddControl(btn);
	btn = new cButton {	Text = "Remove", LocationMargin = new Point(75, 60)};
		btn.Click += Remove_Click;
		AddControl(btn);
}
public void NuevoCnt()										{	s_LimpiaPropsImg(); m_Cierra(); m_AddFme(0); lbFmes.Load(Frames, null, true);}
public bool AbreCnt(string sRuta)							{	s_LimpiaPropsImg(); return m_bAddCnt(sRuta, true);}
public bool GrabaCnt(string sRuta)
{try
{	cBitmap.Save(sRuta, default, new cBitmap.cExtradata {	Format = mMod.Cfg.Format, IsCube = mMod.Cfg.EsCubo}, Frames.Count
		, (cBitmap.cFrameEncoder fe) =>
		{	bool bMultiFme;
		
			bMultiFme = mImaging.IsMultiframe(fe.ContainerFormat);
			_ = fe.WriteContainerMetadata(Frames[0].Metadata);
			foreach (fVista vis in Frames)
			{	cBitmap.MetadataWriter? mw;
			
				fe.Create();
				mw = fe.WriteMetadata(vis.Metadata);
					if (mw is not null)
					{	if (fe.ContainerFormat == cBitmap.eContainerFormat.Jpeg)
							mw.Value.Write("/app1/ifd/{ushort=274}", 1);
						else if (fe.ContainerFormat == cBitmap.eContainerFormat.Tiff)
							mw.Value.Write("/ifd/{ushort=274}", 1);
					}
				vis.EscribeFme(fe);	if (!bMultiFme)	break;					// Escribir fme.  Si es el único: saltar
			}
			mMod.Cfg.ContainerFormat = fe.ContainerFormat;					// Actualizar cnt fmt
		});
	foreach (fVista vis in Frames)	vis.Changed = false;					// Ya no hay cambio
} catch (System.Exception ex)
{	mDialog.MsgBoxError(ex.Message + "\nCheck:\nPixel format\nSize of all frames\nThe size must be a multiple of 4 for BC formats");
	return false;
}
	return true;
}
private void New_Click(object sender)						{	m_AddFme(Frames.Count); lbFmes.Load(Frames, null, true); mMod.MainWnd.Changed = true;}
private void Open_Click(object sender)
{	string? s;

	s = mDialog.ShowOpenFile(mMod.FILTRO_ABRIR, mMod.DLG_IMG_GUID);	if (s is null) return;
	_ = m_bAddCnt(s, false); mMod.MainWnd.Changed = true;
}
private void Remove_Click(object sender)
{	fVista? vis = (fVista?)lbFmes.SelectedValue;

		if (Frames.Count == 1 || vis is null ||
				!mDialog.MsgBoxQuestion(string.Format("Remove frame {0}?  This operation can't be undone", vis.Text), "Remove"))
		{	return;
		}
	vis.Close(); Frames.Remove(vis);
	for (int i = 0; i < Frames.Count; i++)	Frames[i].Text = (i + 1).ToString();
	lbFmes.Load(Frames, null, true); mMod.MainWnd.Changed = true;
}
private void m_Cierra()
{	foreach (fVista vis in Frames)	vis.Close();
	Frames.Clear(); lbFmes.Load(null);
}
private bool m_bAddCnt(string sRuta, bool bEsPrincipal)
{	cBitmap bmpArchi;														// Bmp sin dispose

try
{	bmpArchi = new cBitmap(sRuta, true);
	if (bEsPrincipal)
	{	mMod.Cfg.ContainerFormat = bmpArchi.ContainerFormat;
		if (bmpArchi.Extradata is not null)	{	mMod.Cfg.Format = bmpArchi.Extradata.Format; mMod.Cfg.EsCubo = bmpArchi.Extradata.IsCube;} // Tomar extradata
		m_Cierra();
	}
	for (int i = 0; i < bmpArchi.FrameCount; i++)	m_AddFme(Frames.Count, bmpArchi[i]); // ** Mostrar
	bmpArchi.ClearFrames();													// Quitar fmes del principal para no elim los demás al elim el 1°
	lbFmes.Load(Frames, null, true);
} catch (System.Exception ex)
{	mDialog.MsgBoxError(ex);
	return false;
}
	return true;
}
private void m_AddFme(int iNum, cBitmap? bmpBmp = null)
{	fVista vis;

	vis = new fVista(bmpBmp) {	Container = this.Container, Text = (iNum + 1).ToString()};
	Frames.Add(vis);
	vis.Show();
}
}