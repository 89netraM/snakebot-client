using Zarya;
using Zarya.Input;

#nullable disable

namespace Mårten.Snake.Services;

public record InputService(IInputManager InputManager) : InputBase(InputManager)
{
	[KeyboardInput(Key.Up, InputState.Pressed)]
	[KeyboardInput(Key.W, InputState.Pressed)]
	[GamepadButtonInput(Button.DPadUp, InputState.Pressed)]
	public InputButton Up { get; }

	[KeyboardInput(Key.Left, InputState.Pressed)]
	[KeyboardInput(Key.A, InputState.Pressed)]
	[GamepadButtonInput(Button.DPadLeft, InputState.Pressed)]
	public InputButton Left { get; }

	[KeyboardInput(Key.Right, InputState.Pressed)]
	[KeyboardInput(Key.D, InputState.Pressed)]
	[GamepadButtonInput(Button.DPadRight, InputState.Pressed)]
	public InputButton Right { get; }

	[KeyboardInput(Key.Down, InputState.Pressed)]
	[KeyboardInput(Key.S, InputState.Pressed)]
	[GamepadButtonInput(Button.DPadDown, InputState.Pressed)]
	public InputButton Down { get; }

	[KeyboardInput(Key.Enter, InputState.Down)]
	[KeyboardInput(Key.Space, InputState.Down)]
	[GamepadButtonInput(Button.Start, InputState.Down)]
	[GamepadButtonInput(Button.South, InputState.Down)]
	public InputButton Start { get; }
}
