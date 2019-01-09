namespace Zios.File.FileEvents{
	using Zios.Events;
	using Zios.File;
	using Zios.Unity.Proxy;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	public static class FileEvents{
		static FileEvents(){
			File.refreshHooks += FileEvents.Refresh;
		}
		public static void Refresh(){
			if(Proxy.IsEditor()){
				Events.Add("On Editor Update",File.Monitor).SetPermanent();
				Events.Add("On Asset Changed",File.Refresh).SetPermanent();
				Events.Add("On Mode Changed",File.CheckSave).SetPermanent();
			}
		}
	}
}