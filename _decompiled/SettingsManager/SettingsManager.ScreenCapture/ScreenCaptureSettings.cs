using FluxDB;

namespace SettingsManager.ScreenCapture;

public class ScreenCaptureSettings(DbManager dbManager)
{
	public AcceptParty AcceptParty { get; set; } = new AcceptParty(dbManager);

	public CureDB CureDB { get; set; } = new CureDB(dbManager);

	public HandlePartyMemberDeath HandlePartyMemberDeath { get; set; } = new HandlePartyMemberDeath(dbManager);

	public PartyMemberCount PartyMemberCount { get; set; } = new PartyMemberCount(dbManager);

	public RepairArmors RepairArmors { get; set; } = new RepairArmors(dbManager);

	public RepairWeapons RepairWeapons { get; set; } = new RepairWeapons(dbManager);

	public ReReRe ReReRe { get; set; } = new ReReRe(dbManager);

	public StopMacrosOnGenieStop StopMacrosOnGenieStop { get; set; } = new StopMacrosOnGenieStop(dbManager);

	public SwapTomahawk SwapTomahawk { get; set; } = new SwapTomahawk(dbManager);

	public RectanglesSettings RectanglesSettings { get; set; } = new RectanglesSettings(dbManager);

	public InventorySlotAlert InventorySlotAlert { get; set; } = new InventorySlotAlert(dbManager);

	public StartGenie StartGenie { get; set; } = new StartGenie(dbManager);

	public WhellOfFun WhellOfFun { get; set; } = new WhellOfFun(dbManager);
}
