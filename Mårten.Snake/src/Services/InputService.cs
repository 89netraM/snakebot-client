using Zarya;
using Zarya.Input;

#nullable disable

namespace Mårten.Snake.Services;

public record InputService(IInputManager InputManager) : InputBase(InputManager)
{
	[KeyboardInput(Key.Up, InputState.Down)]
	[KeyboardInput(Key.W, InputState.Down)]
	[GamepadButtonInput(Button.DPadUp, InputState.Down)]
	public InputButton Up { get; }

	[KeyboardInput(Key.Left, InputState.Down)]
	[KeyboardInput(Key.A, InputState.Down)]
	[GamepadButtonInput(Button.DPadLeft, InputState.Down)]
	public InputButton Left { get; }

	[KeyboardInput(Key.Right, InputState.Down)]
	[KeyboardInput(Key.D, InputState.Down)]
	[GamepadButtonInput(Button.DPadRight, InputState.Down)]
	public InputButton Right { get; }

	[KeyboardInput(Key.Down, InputState.Down)]
	[KeyboardInput(Key.S, InputState.Down)]
	[GamepadButtonInput(Button.DPadDown, InputState.Down)]
	public InputButton Down { get; }

	[KeyboardInput(Key.Enter, InputState.Down)]
	[KeyboardInput(Key.Space, InputState.Down)]
	[GamepadButtonInput(Button.Start, InputState.Down)]
	[GamepadButtonInput(Button.South, InputState.Down)]
	public InputButton Start { get; }
}
