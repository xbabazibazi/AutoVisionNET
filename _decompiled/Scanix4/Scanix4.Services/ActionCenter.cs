using InputManager;
using Scanix4.Services.ActionCategories;
using SimpleLogger;

namespace Scanix4.Services;

public class ActionCenter(Alarm alarm, Logger logger, InputUtils inputUtils)
{
	public readonly RequestActions _requestActions = new RequestActions(alarm, logger, inputUtils);

	public readonly PartyActions _partyActions = new PartyActions(alarm, logger, inputUtils);

	public readonly GenieActions _genieActions = new GenieActions(alarm, logger, inputUtils);

	public readonly BuffLineActions _buffLineActions = new BuffLineActions(alarm, logger, inputUtils);

	public readonly WeaponsActions _weaponsActions = new WeaponsActions(alarm, logger, inputUtils);

	public readonly MagicBagActions _magicBagActions = new MagicBagActions(alarm, logger, inputUtils);

	public readonly InventortyActions _inventortyActions = new InventortyActions(alarm, logger, inputUtils);
}
