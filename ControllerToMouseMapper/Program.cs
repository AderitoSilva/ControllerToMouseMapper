using ControllerToMouseMapper.Input;
using XInputium.ModifierFunctions;
using XInputium.XInput;

// For this sample project, we are using our own simple implementation of mouse and
// keyboard interop. But for more complete and capable keyboard and mouse simulation
// features, check out: https://github.com/MediatedCommunications/WindowsInput/

// NOTE: To control the mouse and/or the keyboard when privileged applications have
// the focus, our application must also be run with privileges.

// NOTE: For the input loop, we are using a simple implementation of a precise timer,
// which is CPU intensive. In production, you should use something more appropriate,
// like a native media timer.

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
mouseJoystick.RadiusModifierFunction = NonLinearFunctions.CubicEaseIn;  // Make the joystick more precise near the center.

// Map the joystick to the mouse.
double moveSpeed = 0.01;  // Move speed, in screen ratio per second (kind of).
mouseJoystick.Updated += (_, _) =>
{
    // Update the mouse on every input loop update. If the joystick is at rest position,
    // we don't need to move the mouse. Notice we are not using the joystick move event,
    // because we want the mouse to keep moving while the joystick is being held (pushed),
    // and not just when it is moved.
    if (mouseJoystick.IsPushed)
    {
        Mouse.MoveBy(
            mouseJoystick.X * moveSpeed * mouseJoystick.FrameTime.TotalSeconds,
            -mouseJoystick.Y * moveSpeed * mouseJoystick.FrameTime.TotalSeconds);
    }
};

// Map controller buttons to the mouse buttons.
leftMouseButton.Pressed += (_, _) => Mouse.PressButton(MouseButton.Left);
leftMouseButton.Released += (_, _) => Mouse.ReleaseButton(MouseButton.Left);
rightMouseButton.Pressed += (_, _) => Mouse.PressButton(MouseButton.Right);
rightMouseButton.Released += (_, _) => Mouse.ReleaseButton(MouseButton.Right);
middleMouseButton.Pressed += (_, _) => Mouse.PressButton(MouseButton.Middle);
middleMouseButton.Released += (_, _) => Mouse.ReleaseButton(MouseButton.Middle);

// Map controller buttons to keyboard keys.
TimeSpan buttonRepeatDelay = TimeSpan.FromMilliseconds(350);  // When a button is held, how long until it starts repeating?
TimeSpan buttonRepeatInterval = TimeSpan.FromMilliseconds(90);  // While a held button is repeating, how long is the interval?
gamepad.RegisterButtonRepeatEvent(XButtons.DPadUp, buttonRepeatDelay, buttonRepeatInterval,
    (_, _) => Keyboard.TapKey(KeyboardVirtualKey.Up));
gamepad.RegisterButtonRepeatEvent(XButtons.DPadDown, buttonRepeatDelay, buttonRepeatInterval,
    (_, _) => Keyboard.TapKey(KeyboardVirtualKey.Down));
gamepad.RegisterButtonRepeatEvent(XButtons.DPadLeft, buttonRepeatDelay, buttonRepeatInterval,
    (_, _) => Keyboard.TapKey(KeyboardVirtualKey.Left));
gamepad.RegisterButtonRepeatEvent(XButtons.DPadRight, buttonRepeatDelay, buttonRepeatInterval,
    (_, _) => Keyboard.TapKey(KeyboardVirtualKey.Right));

gamepad.RegisterButtonRepeatEvent(XButtons.LB, buttonRepeatDelay, buttonRepeatInterval,
    (_, _) =>
    {
        // Shift+Tab
        Keyboard.PressKey(KeyboardVirtualKey.Shift);
        Keyboard.TapKey(KeyboardVirtualKey.Tab);
        Keyboard.ReleaseKey(KeyboardVirtualKey.Shift);
    });
gamepad.RegisterButtonRepeatEvent(XButtons.RB, buttonRepeatDelay, buttonRepeatInterval,
    (_, _) => Keyboard.TapKey(KeyboardVirtualKey.Tab));

gamepad.Buttons.B.Pressed += (_, _) => Keyboard.TapKey(KeyboardVirtualKey.Escape);


// Start the input loop. Note that, from now on, any changes you may want to make to
// the gamepad must always be done within the input loop update callback, to ensure
// thread safe access to the gamepad.
using InputLoop inputLoop = new(time =>
{
    gamepad.Update();
    if (!gamepad.IsConnected)
    {
        gamepad.Device = XInputDevice.GetFirstConnectedDevice();
    }
});
inputLoop.Start();


// =====================================

// Wait for the user to press any key on the console, before exiting.
Console.WriteLine();
Console.WriteLine("Press any key to terminate the application.");
Console.ReadKey();

