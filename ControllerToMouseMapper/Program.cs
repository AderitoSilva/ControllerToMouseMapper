using ControllerToMouseMapper.Input;
using XInputium.ModifierFunctions;
using XInputium.XInput;

// For this sample project, we are using our own simple implementation of mouse and
// keyboard interop. But for more complete and capable keyboard and mouse simulation
// features, check out: https://github.com/MediatedCommunications/WindowsInput/

// Show some initial text in the console.
Console.WriteLine("This application maps the controller to the mouse. " +
    "This is just a demo application, with minimal functionality.");
Console.WriteLine();
Console.WriteLine("--Left joystick: mouse movement");
Console.WriteLine("--A: Left mouse button");
Console.WriteLine("--X: Right mouse button");
Console.WriteLine("--LS: Middle mouse button");
Console.WriteLine("--B: Escape key");
Console.WriteLine("--LB: Shift+Tab key");
Console.WriteLine("--LR: Tab key");
Console.WriteLine("--DPad: Arrow keys");

// =========================================================


XGamepad gamepad = new();

var mouseJoystick = gamepad.LeftJoystick;
var leftMouseButton = gamepad.Buttons.A;
var rightMouseButton = gamepad.Buttons.X;
var middleMouseButton = gamepad.Buttons.LS;

// Configure the mouse joystick.
mouseJoystick.InnerDeadZone = 0.2f;  // Let's use a circular dead zone.
mouseJoystick.RadiusModifierFunction = NonLinearFunctions.QuadraticEaseIn;  // Make the joystick more precise near the center.

// Map the joystick to the mouse.
double moveSpeed = 0.01;  // Move speed, in screen ratio per second.
mouseJoystick.Updated += (sender, e) =>
{
    // Update the mouse on every input loop update. If the joystick is at rest position,
    // we don't need to move the mouse. Notice we are not using the joystick move event,
    // because we want the mouse to keep moving while the joystick is being held (pushed).
    if (mouseJoystick.IsPushed)
    {
        Mouse.MoveBy(
            mouseJoystick.X * moveSpeed * gamepad.FrameTime.TotalSeconds,
            -mouseJoystick.Y * moveSpeed * gamepad.FrameTime.TotalSeconds);
    }
};

// Map controller buttons to the mouse buttons.
leftMouseButton.Pressed += (sender, e) => Mouse.PressButton(MouseButton.Left);
leftMouseButton.Released += (sender, e) => Mouse.ReleaseButton(MouseButton.Left);
rightMouseButton.Pressed += (sender, e) => Mouse.PressButton(MouseButton.Right);
rightMouseButton.Released += (sender, e) => Mouse.ReleaseButton(MouseButton.Right);
middleMouseButton.Pressed += (sender, e) => Mouse.PressButton(MouseButton.Middle);
middleMouseButton.Released += (sender, e) => Mouse.ReleaseButton(MouseButton.Middle);

// Map controller buttons to keyboard keys.
TimeSpan buttonRepeatDelay = TimeSpan.FromMilliseconds(350);
TimeSpan buttonRepeatInterval = TimeSpan.FromMilliseconds(100);
gamepad.RegisterButtonRepeatEvent(XButtons.DPadUp, buttonRepeatDelay, buttonRepeatInterval,
    (_, e) => Keyboard.TapKey(KeyboardVirtualKey.Up));
gamepad.RegisterButtonRepeatEvent(XButtons.DPadDown, buttonRepeatDelay, buttonRepeatInterval,
    (_, e) => Keyboard.TapKey(KeyboardVirtualKey.Down));
gamepad.RegisterButtonRepeatEvent(XButtons.DPadLeft, buttonRepeatDelay, buttonRepeatInterval,
    (_, e) => Keyboard.TapKey(KeyboardVirtualKey.Left));
gamepad.RegisterButtonRepeatEvent(XButtons.DPadRight, buttonRepeatDelay, buttonRepeatInterval,
    (_, e) => Keyboard.TapKey(KeyboardVirtualKey.Right));

gamepad.RegisterButtonRepeatEvent(XButtons.LB, buttonRepeatDelay, buttonRepeatInterval,
    (_, e) =>
    {
        Keyboard.PressKey(KeyboardVirtualKey.Shift);
        Keyboard.TapKey(KeyboardVirtualKey.Tab);
        Keyboard.ReleaseKey(KeyboardVirtualKey.Shift);
    });
gamepad.RegisterButtonRepeatEvent(XButtons.RB, buttonRepeatDelay, buttonRepeatInterval,
    (_, e) => Keyboard.TapKey(KeyboardVirtualKey.Tab));

gamepad.Buttons.B.Pressed += (sender, e) => Keyboard.TapKey(KeyboardVirtualKey.Escape);


// Start the input loop. Note that any changes you may want to make on the gamepad must
// always be done within the input loop update callback, to ensure thread safety.
using InputLoop inputLoop = new(() =>
{
    gamepad.Update();
    if (!gamepad.IsConnected)
    {
        gamepad.Device = XInputDevice.GetFirstConnectedDevice();
    }
});

// =====================================

// Wait for the user to press any key on the console, before exiting.
Console.WriteLine();
Console.WriteLine("Press any key to terminate the application.");
Console.ReadKey();
