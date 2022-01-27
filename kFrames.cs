using Wew.Control;
using Wew.Media;
using Wew;

namespace DirectPaint
{
class kFrames : cDockControl
{	readonly cList<fVista> ml_visFmes;
	readonly cListBox lbFmes;
public kFrames()
{	cButton btn;

	Width = 230; Text = "Frames"; IsChildForm = false; HideOnClose = true; Dock = eDirection.Left;
	ml_visFmes = new cList<fVista>();
	lbFmes = new cListBox() {	Size = new Point(120, 450), Data = ml_visFmes};
		AddControl(lbFmes);
	btn = new cButton() {	Text = "New", LocationMargin = new Point(125, 0)};
		btn.Click += New_Click;
		AddControl(btn);
	btn = new cButton() {	Text = "Open", LocationMargin = new Point(125, 30)};
		btn.Click += Open_Click;
		AddControl(btn);
	btn = new cButton() {	Text = "Remove", LocationMargin = new Point(125, 60)};
		btn.Click += Remove_Click;
		AddControl(btn);
}
public void Nuevo()											{	m_LimpiaPropsImg(); m_Cierra(); m_Add(0); lbFmes.Refresh();}
public bool Abre(string sRuta)								{	m_LimpiaPropsImg(); return m_bAdd(sRuta, true);}
public bool Graba(string sRuta)
{	cBitmap bmpImg = null; System.IntPtr ptrDds = System.IntPtr.Zero;

try
{	// ** Dds
	if (System.IO.Path.GetExtension(sRuta).ToUpperInvariant() == ".DDS")
	{	PointI pti; int iStride, iTamFme; System.IntPtr ptr;
	
		pti = ml_visFmes[0].Tam.GetRounded(); iStride = pti.X * 4; iTamFme = iStride * pti.Y; // ** Unir fmes
		ptrDds = ptr = mHelper.Malloc(iTamFme * ml_visFmes.Count);
		foreach (fVista vis in ml_visFmes)
		{	using (cBitmap bmp = vis.CompónImg())
			{		if (bmp.PixelSize != pti)	{	mDialog.MsgBoxExclamation("All frames must be the same size", "Save"); return false;}
				bmp.PixelFormat = ePixelFormat._32bppBGRA; bmp.CopyTo(RectangleI.Empty, iStride, ptr, iTamFme); ptr += iTamFme;
			}
		}
		cTexture.Save(sRuta, ptrDds, (iTamFme * ml_visFmes.Count), ml_visFmes.Count, pti, mMod.Cfg.Format, mMod.Cfg.EsCubo); // Grabar
	// ** Img
	} else
	{	foreach (fVista vis in ml_visFmes)									// ** Unir fmes
		{	cBitmap bmp;
		
			bmp = vis.CompónImg(); bmp.Metadata = vis.Metadata;
			if (bmpImg == null)	bmpImg = bmp;	else	bmpImg.InsertFrame(bmp);
		}
		bmpImg.Save(sRuta);													// Grabar
	}
	foreach (fVista vis in ml_visFmes)	vis.Changed = false;
} catch (System.Exception ex)
{	mDialog.MsgBoxExclamation(ex.Message, "Error");
	return false;
} finally
{	if (bmpImg != null)
	{	for (int i = 0; i < bmpImg.FrameCount; i++)	bmpImg[i].Metadata = null; // No perder metas
		bmpImg.Dispose();
	}
	if (ptrDds != System.IntPtr.Zero)	mHelper.Free(ptrDds);
}
	return true;
}
private void New_Click(object sender)						{	m_Add(ml_visFmes.Count); lbFmes.Refresh(); mMod.MainWnd.Changed = true;}
private void Open_Click(object sender)
{	string s;

	s = mDialog.ShowOpenFile(mMod.FILTRO_ABRIR, mMod.DLG_IMG_GUID);	if (s == null) return;
	m_bAdd(s, false); mMod.MainWnd.Changed = true;
}
private void Remove_Click(object sender)
{	fVista vis = (fVista)lbFmes.SelectedItem;

		if (ml_visFmes.Count == 1 || vis == null ||
			!mDialog.MsgBoxQuestion(string.Format("Remove frame {0}?  This operation can't be undone", vis.Text), "Remove"))	return;
	vis.Close(); ml_visFmes.Remove(vis);
	for (int i = 0; i < ml_visFmes.Count; i++)	ml_visFmes[i].Text = (i + 1).ToString();
	lbFmes.Refresh(); mMod.MainWnd.Changed = true;
}
private void m_Cierra()
{	foreach (fVista vis in ml_visFmes)	vis.Close();
	ml_visFmes.Clear(); lbFmes.Refresh();
}
private void m_LimpiaPropsImg()
{	mMod.Cfg.Format = cTexture.eFormat.BC1_R5G6B5A1; mMod.Cfg.EsCubo = false;
}
private bool m_bAdd(string sRuta, bool bCerrarActual)
{	cBitmap bmpArchi = null; System.IntPtr ptrDds = System.IntPtr.Zero;

try
{	using (System.IO.Stream stm = System.IO.File.Open(sRuta					// Abrir incluso si el archivo está bloqueado
		, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
	{	// ** Dds
		if (stm.PeekInt() == 0x20534444)									// "DDS "
		{	System.IntPtr ptr; int iFmes; PointI ptiTam; cTexture.eFormat fmt; bool bCubo; cBitmap bmp;

			ptrDds = ptr = cTexture.Open(stm, (int)stm.Length, out iFmes, out ptiTam, out fmt, out bCubo);
			mMod.Cfg.Format = fmt; mMod.Cfg.EsCubo = bCubo;
			for (int i = 0, iStride = ptiTam.X * 4, iTamFme = iStride * ptiTam.Y; i < iFmes; i++, ptr += iTamFme)
			{	bmp = new cBitmap(ptiTam.X, ptiTam.Y, ePixelFormat._32bppBGRA, iStride, ptr, iTamFme);
				if (bmpArchi == null)	bmpArchi = bmp;	else	bmpArchi.InsertFrame(bmp);
			}
		// ** Img
		} else
			bmpArchi = new cBitmap(stm, (int)stm.Length, true);
	}
	if (bCerrarActual)	m_Cierra();
	for (int i = 0; i < bmpArchi.FrameCount; i++)	m_Add(ml_visFmes.Count, bmpArchi[i]); // ** Mostrar
	lbFmes.Refresh();
} catch (System.Exception ex)
{	mDialog.MsgBoxExclamation(ex.Message, "Error");
	return false;
} finally
{	if (ptrDds != System.IntPtr.Zero)	mHelper.Free(ptrDds);
}
	return true;
}
private void m_Add(int iNum, cBitmap bmpBmp = null)
{	fVista vis;

	vis = new fVista(bmpBmp) {	Container = this.Container, Text = (iNum + 1).ToString()};
	ml_visFmes.Add(vis);
	vis.Show();
}
}
}