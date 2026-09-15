using System.Drawing;
using System.Threading;
using InputManager;
using SimpleLogger;

namespace Scanix4.Services.BaseClasses;

public class BaseActions
{
	protected readonly InputUtils _inputUtils;

	protected readonly Logger Logger;

	private const int DEFAULT_CLICK_DELAY = 150;

	private const int DEFAULT_WAIT_DELAY = 250;

	private const int DEFAULT_MOVE_DELAY = 250;

	private const int CURSOR_SPEED = 50;

	public BaseActions(InputUtils inputUtils, Logger logger)
	{
		_inputUtils = inputUtils;
		Logger = logger;
	}

	public void MoveAndRightClick(Point coordinates)
	{
		_inputUtils.SimulateMoveTo(coordinates.X, coordinates.Y);
		Thread.Sleep(250);
		_inputUtils.SimulateRightButtonClick(150);
		Thread.Sleep(250);
		MoveSafeArea();
	}

	public void MoveSafeArea()
	{
		_inputUtils.SimulateMoveTo(400, 400);
	}

	public void SimulateRightClick(int clickDelay = 150, int waitDelay = 250)
	{
		_inputUtils.SimulateRightButtonClick(clickDelay);
		Thread.Sleep(waitDelay);
	}

	public void SimulateRightClick(Point coordinates, int clickDelay = 150, int waitDelay = 250)
	{
		_inputUtils.SimulateRightButtonClick(clickDelay);
		Thread.Sleep(waitDelay);
	}

	public void SimulateLeftClick(int clickDelay = 150, int waitDelay = 250)
	{
		_inputUtils.SimulateLeftButtonClick(clickDelay);
		Thread.Sleep(waitDelay);
	}

	public void MoveAndDoubleLeftClick(Point coordinates)
	{
		_inputUtils.SimulateMoveTo(coordinates.X, coordinates.Y);
		SimulateLeftClick();
		SimulateLeftClick();
		Thread.Sleep(250);
		MoveSafeArea();
	}

	public void MoveAndLeftClick(Point coordinates)
	{
		_inputUtils.SimulateMoveTo(coordinates.X, coordinates.Y);
		Thread.Sleep(250);
		SimulateLeftClick();
		MoveSafeArea();
	}

	public void MoveAndLeftButtonDown(Point coordinates)
	{
		_inputUtils.SimulateMoveTo(coordinates.X, coordinates.Y);
		Thread.Sleep(300);
		_inputUtils.SimulateLeftButtonDown();
		Thread.Sleep(300);
		MoveSafeArea();
	}

	public void MoveAndLeftButtonUp(Point coordinates)
	{
		_inputUtils.SimulateMoveTo(coordinates.X, coordinates.Y);
		Thread.Sleep(300);
		_inputUtils.SimulateLeftButtonUp();
		Thread.Sleep(300);
		MoveSafeArea();
	}

	public void LeftButtonUp()
	{
		_inputUtils.SimulateLeftButtonUp();
		Thread.Sleep(300);
		MoveSafeArea();
	}
}
