using FluxDB;

namespace SettingsManager.Macro;

public class MacroSettings(DbManager dbManager)
{
	public Attack Attack { get; set; } = new Attack(dbManager);

	public Escape Escape { get; set; } = new Escape(dbManager);

	public General General { get; set; } = new General(dbManager);

	public Login Login { get; set; } = new Login(dbManager);

	public TpParty TpParty { get; set; } = new TpParty(dbManager);

	public UndyAc UndyAc { get; set; } = new UndyAc(dbManager);
}
