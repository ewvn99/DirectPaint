namespace DirectPaint;

static class Program
{
[System.STAThread] static void Main()						{	

	//Wew.mHelper.SetBrkId(519);

	Wew.Control.cWindow.Main(() => new wMain());
	
	//foreach (var item in typeof(mRes).GetFields())		((System.IDisposable)item.GetValue(null)!).Dispose();
	//mMod.FondoTransp.Dispose(); mMod.GridLin.Dispose();
	////kHerrs.Fuente.Dispose();
	//mMod.Colores.Fuente.Dispose();
	//mMod.Cfg.Trazo.Dispose();
	//cUndo.Surf?.Dispose();

	//Wew.mHelper.DisposeAll();
	
	}
}