# What is TDD?
> Test-driven development (TDD) is a software development process that relies on the repetition of a very short development cycle: first the developer writes a failing automated test case that defines a desired improvement or new function, then produces code to pass that test and finally refactors the new code to acceptable standards.

# Why do TDD?
* Forces you to adopt the perspective of the *user* -- both of your code and of your application.
* Forces you to describe the intended behavior *prior* to implementing the behavior, making it more difficult for behavior to arise from happenstance. ("Interface before implementation.")
* Produces less ambiguous documentation that is less likely to become stale.
* Reduces your dependence on debuggers.
* Lets you move faster.
* It tends to be harder to write bad code.
* Classes tend to be smaller, more focused, and more loosely coupled, with cleaner interfaces.
* Free regression testing.
* It's cheaper and easier to fix bugs earlier.
* Allows sustainable development. (As long as all the tests pass, the project is shippable. No crunch time, no "bugfixing phase", no "maintenance phase" since maintenance is the same as continued development.)
* Easier bugfix workflow: Write a test case to represent the scenario, see it fail, fix the bug, see it pass.
* Higher code coverage than testing after the fact.
* Increases confidence in the codebase.
* Lets you refactor ruthlessly.
* Helps you adapt to changing requirements.
* Allows newbie developers to write decent code.
* Allows non-technical members to interact with the codebase by writing plain-English test cases.
* Provides visibility of the current state of the project.
* And, of course, verifies the correctness of your code (which you would be doing anyway).
* Also, it's more fun!

# What is a test?
A test is simply an assertion about the behavior of a particular piece of code.

    [TestFixture]
    public class CalculatorTest
    {
        [Test]
        public void AddTest()
        {
            var calculator = new Calculator();
            var result = calculator.Add(3, 8);
            Assert.That(result, Is.EqualTo(11));
        }
    }

Output before we implement Add():
    F
    Tests run: 1, Errors: 0, Failures: 1, Inconclusive: 0, Time: 0.337824 seconds
      Not run: 0, Invalid: 0, Ignored: 0, Skipped: 0

    Errors and Failures:
    1) Test Failure : Calculator.Test.CalculatorTest.AddTest
         Expected: 11
         But was:  0

Output after we implement Add():
    .
    Tests run: 1, Errors: 0, Failures: 0, Inconclusive: 0, Time: 0.337824 seconds
      Not run: 0, Invalid: 0, Ignored: 0, Skipped: 0

# How do I test?
There are two main approaches to testing. They are characterized as "state-based" (or black-box) and "interaction-based" (or white-box). They offer complementary benefits, and are often used in conjunction to fully test a feature.

## State-based testing
State-based testing relies on putting the object under test into a known state, triggering a state change, and then asserting some facts about its new state.

    var door = new Door(DoorState.Closed); // Door is initially closed.
    door.Open();
    Assert.That(door.State, Is.EqualTo(DoorState.Open));

The earlier example of the calculator is also a state-based test.

    var calculator = new Calculator();
    var result = calculator.Add(3, 8);
    Assert.That(result, Is.EqualTo(11));

## Interaction-based testing
Interaction-based testing, on the other hand, specifies how an object interacts with its dependencies. Here, we are less concerned with the state of any particular object -- we just want to ensure that the right calls are being made with the right arguments.

In this example, we are testing our DamageDealer class, which will ask our guy how much armor he has and then deal him the remaining damage.

    var guy = new Mock<Guy>(); // Create a mock guy.
    guy.Setup(guy => guy.Armor).Returns(50); // We have 50 armor.
    guy.Setup(guy => guy.TakeDamage(25)); // Create an expectation that guy.TakeDamage will be called with 25 as its argument.
    var damageDealer = new DamageDealer();
    damageDealer.DealDamage(guy, 75); // Deal 75 damage, which becomes 25 damage after armor.

We could test this using a real instance of Guy, but if Armor is calculated based on his level, skill points, equipment, and so on, it might be overly complicated to set up the correct scenario and force us to revisit the test if the Armor calculation ever changes -- that's not what we want at all.

Using a mock allowed us to only test the things that actually matter to us, protecting us from the implementation details of our dependencies and making it easier to set up our scenario. It comes at the expense of writing "fake" code -- if this were the only test that involved Guy.TakeDamage(), and that method was totally broken, we would not catch it because we are mocking it out.

# The kinds of tests
There are three categories that you'll hear most often with respect to testing: integration tests, functional tests, and unit tests. They each adopt a different perspective and have slightly different goals.

## Integration tests
Integration tests are the highest-level tests. They take the user's perspective and walk through an entire feature. They form the acceptance criteria for a feature -- when they are all passing, the feature is considered done. They tend to be written in a language that a non-technical stakeholder can understand, such as Cucumber.

    Scenario: Scoring a point
        Given I am playing a game of Pong
        And I have a ball coming at me
        And I hit the ball
        When my opponent misses the ball
        Then my score should be 1

They tend to only cover "happy path" cases, talk about "visible" outcomes rather than raw data structures, and almost never use mocks.

## Functional tests
A functional test represents the user performing a discrete interaction with the system. For a web app, this is submitting a form, clicking a button, or requesting a web page. For a game, it means moving your character, using a skill, or wielding your weapon.

Functional tests are written from the perspective of the system itself, may cover degenerate cases, deal with high-level data structures, and may use mocks to a limited extent.

    var shootingGame = LoadShootingGameWithMeLookingAtEnemy1();
    Assert.That(shootingGame.Enemies.First().IsAlive);
    Assert.That(shootingGame.MyAmmo, Is.EqualTo(50));
    shootingGame.Shoot();
    shootingGame.ProcessTime(10); // 10ms pass.
    Assert.That(shootingGame.Enemies.First().IsDead);
    Assert.That(shootingGame.MyAmmo, Is.EqualTo(49));

Functional tests form the specification of your high-level API. Here, you can see that I have a pre-constructed scenario to put the game into the right state for me to explode some poor sap's face. I make a couple of sanity checks, call the high-level method that represents the action the player is taking, simulate 10ms of game time to allow my explosion to take effect, then check that my ammo went down and so did the bad guy.

## Unit tests
Unit tests are the lowest-level tests, and are used to describe individual methods on an object. Unit tests tend to make heavier use of interaction-based testing. They goal is to cover every possible code path of a method, so most methods will have multiple test cases associated with them, covering both the normal and degenerate cases. This is where most of the bug-catching happens; it's also where we sketch out the interface of a class.

    Assert.That(rock.Beats(scissors));
    Assert.False(rock.Beats(rock));
    Assert.That(() => rock.Beats(null), Throws.Exception);

# TDD in the wild
Now let's try test-driving Pong.

## 92dec49
This is our starting point. We've got a test environment set up and three failing tests.

### GameStart.feature
I picked a simple rule to start with: when two players join the game, the game starts.

    Feature: Game starting

    Scenario: The game starts when two players join
        Given I have a game of Pong
        When two players join
        Then the game starts

### JoinTest.cs
I broke that integration test down into a single functional test. I put it in a file called JoinTest.cs, which I anticipate will contain every test related to the PongGame.Join() method.

    [Test]
    public void Last_player_joining_starts_game()
    {
        Game.Join(new Player());
        Game.Join(new Player());
        Assert.That(Game.HasStarted);
    }

Notice that I'm using really high-level vocabulary here, using PongGame as my interface to any action I want to take or question I want to ask.

### PongGameTest.cs
I further broke that functional test down into the first unit test.

    [Test]
    public void Join_adds_player_to_players_list()
    {
        var player1 = new Player();
        var player2 = new Player();
        Game.Join(player1);
        Game.Join(player2);
        Assert.That(Game.Players, Is.EqualTo(new Player[] { player1, player2 }));
    }

Not terribly different from the functional test here, which is to be expected since this unit test deals with PongGame.Join() in the same way, but it introduces the concept of the PongGame.Players array. This is a detail that we didn't care about at the functional level, but it's a state change that we do need to care about if we're going to be able to implement the PongGame.HasStarted property.

## 7ab2503
### PongGameTest.cs
Here, I add another unit test to demonstrate the state transition of PongGame.HasStarted

    [Test]
    public void Join_starts_game_when_last_player_joins()
    {
        var player1 = new Player();
        var player2 = new Player();
        Game.Join(player1);
        Game.Join(player2);
        Assert.That(Game.HasStarted);
    }

## 23f89a
### PongGameTest.cs
Here, I made a decision that PongGame would be fed the number of players rather than having it assume there are two, and that the Players array would be full of null references initially.

    [Test]
    public void Constructor_allocates_players_list()
    {
        Game = new PongGame(5);
        Assert.That(Game.Players, Has.Length.EqualTo(5));
        Assert.That(Game.Players, Has.All.Null);
    }

The setup of my other tests changed as a result and I decided to make a helper method SetUp2PlayerPongGame() so I can freely change the constructor of PongGame without having the update every test individually.

### PongGame.cs
After running the tests and seeing them all fail, I implemented the constructor.

    public PongGame(int numberOfPlayers)
    {
        players = Enumerable.Repeat<IPlayer>(null, numberOfPlayers).ToArray();
    }

## ee5b7d
### PongGame.cs
With these two methods, all my tests are now passing.

    public void Join(IPlayer player)
    {
        players[Array.IndexOf(players, null)] = player;
    }

    public bool HasStarted
    {
        get { return players.All(player => players != null); }
    }

Notice that I'm being really cheesy here, deliberately doing *the simplest thing that could possibly work*.

Writing stuff like this can be scary, because it looks like the code is *wrong*, but at the moment it's actually *right*, because all that I claim the system does is say "Yes I've started" after two players have joined. If I went any further, I would be writing functionality that didn't have a test yet.

It's worth mentioning here that it's perfectly okay to aim for the "real" implementation first. If I had written a more rigorous test, or if I had more of the API fleshed out, it would be trivial to stump this cheesy implementation. However, if you're in unfamiliar territory and you're not confident about what to go next, it's always best to "fake it until you make it" and gradually work up to the final implementation.

## d05c58
Okay, now that our system is capable of something, our next goal is to hook it up to some input and a renderer so we can see it in action.

### Main.cs
Sketched out a simple Main that we can build off of later.

    public static void Main(string[] args)
    {
        var game = new PongGame(2);
        var input = new PongInput(new KeyboardInput());
        var display = new PongDisplay();
        while (true)
        {
            input.Apply(game);
            //game.Update(10);
            display.Render(game);
        }
    }

### TestHelper.cs
This section makes heavy use of mocks, so I set up a MockRepository in my TestHelper class that allows me to easily verify my mocks in the test teardown.

    public MockRepository Mocks = new MockRepository(MockBehavior.Strict);

    [TearDown]
    public void TearDown()
    {
        Mocks.VerifyAll();
    }

### PongInputTest.cs
In this test, I've set up a mock object that pretends to be providing information about the state of the keyboard so that I can test more of the input system.

The body of the test goes like this:
n. Player specifies a StartKey, which can be anything -- we just want to see that PongInput asks about the state of the right key.
n. Game expects Join(), receiving the player that we're polling for.
n. Keyboard expects to be asked whether the player's start key is pressed, and will return true.
n. We make the actual call.
n. TearDown() runs, verifying that all of our expected methods got called.

    [SetUp]
    public void SetUp()
    {
        Keyboard = Mocks.Create<IKeyboardInput>();
        Input = new PongInput(Keyboard.Object);
    }

    [Test]
    public void Calls_Join_when_player_presses_start()
    {
        var player = Mocks.Create<IPlayer>();
        var startKey = Key.Enter;
        player.Setup(p => p.StartKey).Returns(startKey);

        var game = Mocks.Create<IPongGame>();
        game.Setup(g => g.Join(player.Object));

        Keyboard.Setup(k => k.IsPressed(startKey)).Returns(true);

        Input.Apply(game.Object);
    }

Test output:

    1) TearDown Error : Pong.Test.PongInputTest.Calls_Join_when_player_presses_start
       TearDown : Moq.MockException : The following setups were not matched:
    IKeyboardInput k => k.IsPressed(Key.Enter)

    IPlayer p => p.StartKey

    IPongGame g => g.Join()

### Problems?
Okay, so now that we've got a test to verify the path from processing keyboard input to joining the game, let's try to make it pass.

    public class PongInput : IPongInput
    {
        public PongInput(IKeyboardInput keyboard)
        {
            this.keyboard = keyboard;
        }

        private IKeyboardInput keyboard;

        public void Apply(IPongGame game)
        {
            // ???
        }
    }

Wait a second, we've got a chicken-and-egg problem. We need a Player in order to check whether we should add a Player. It makes sense for the information "Player 1's start key is <foo>" to live in PongGame, but it doesn't make sense for it to be tied to something that can come and go like Player.

Let's take this opportunity to refactor a bit and say that StartKey lives on a "PlayerSlot" instead.

## 991f32
Okay, introducing the concept of a PlayerSlot changed a lot of classes and some test setups, but all of the previous tests pass just fine with the change. Without my test suite, meager though it may be, I might not have dared to make such a sweeping change in the middle of adding player input.

### IPongGame.cs
Join() has changed to talk about PlayerSlots instead of Players, and we expose an array of PlayerSlots that client code like PongInput can interact with before calling Join().

    public interface IPongGame
    {
        void Join(IPlayerSlot playerSlot);
        bool HasStarted
        {
            get;
        }
        IPlayer[] Players
        {
            get;
        }
        IPlayerSlot[] PlayerSlots
        {
            get;
        }
    }

### Main.cs
Providing the number of players isn't enough anymore, because now we must specify a StartKey for each player slot.

    public static void Main(string[] args)
    {
        var game = new PongGame(
            new PlayerSlot
            {
                StartKey = Key.Num1,
            },
            new PlayerSlot
            {
                StartKey = Key.Num0
            }
        );
        var input = new PongInput(new KeyboardInput());
        var display = new PongDisplay();
        while (true)
        {
            input.Apply(game);
            //game.Update(10);
            display.Render(game);
        }
    }

### PongGame.cs
Lots of changes to this class, almost every line is different. We're still being very cheesy with our implementation, but that's okay. As we just experienced, our designs are frequently wrong, and any speculative code we write might turn out to be a complete waste of time later on.

    public class PongGame : IPongGame
    {
        public PongGame(params IPlayerSlot[] playerSlots)
        {
            foreach (var playerSlot in playerSlots)
            {
                players.Add(playerSlot, null);
            }
        }

        public void Join(IPlayerSlot playerSlot)
        {
            players[playerSlot] = new Player();
        }

        public bool HasStarted
        {
            get { return players.Values.All(player => player != null); }
        }

        public IPlayer[] Players
        {
            get { return players.Values.ToArray(); }
        }

        public IPlayerSlot[] PlayerSlots
        {
            get { return players.Keys.ToArray(); }
        }

        private Dictionary<IPlayerSlot, IPlayer> players = new Dictionary<IPlayerSlot, IPlayer>();
    }

## 44f5c0
Alright, now that we're armed with the power of PlayerSlots, let's pass that test.

### TestHelper.cs
I added a couple of convenience methods to TestHelper.

    public Mock<T> Mock<T>() where T : class
    {
        return Mocks.Create<T>(MockBehavior.Strict);
    }

    public Mock<T> Stub<T>() where T : class
    {
        return Mocks.Create<T>(MockBehavior.Loose);
    }

In testing world terminology, "mocks" differ from "stubs" in that mocks will blow up if you call something they didn't expect, whereas stubs will ignore the call and return a default value. It's a good idea to use mocks for the things you really care about (in our case, the call to Join()) and stubs for the things you don't really care about but need to fake out for one reason or another.

### PongInputTest.cs
I changed the test a little bit to reflect how we intend to use the PlayerSlots array to find out what keys we should be checking.

    [Test]
    public void Calls_Join_when_player_presses_start()
    {
        var playerSlot1 = Stub<IPlayerSlot>();
        playerSlot1.Setup(p => p.StartKey).Returns(Key.Enter);
        var playerSlot2 = Stub<IPlayerSlot>();
        playerSlot2.Setup(p => p.StartKey).Returns(Key.Tab);

        Keyboard.Setup(k => k.IsPressed(Key.Enter)).Returns(true);
        Keyboard.Setup(k => k.IsPressed(Key.Tab)).Returns(false);

        var game = Mock<IPongGame>();
        game.Setup(g => g.PlayerSlots).Returns(new IPlayerSlot[] { playerSlot1.Object, playerSlot2.Object });
        game.Setup(g => g.Join(playerSlot1.Object));

        Input.Apply(game.Object);
    }

### PongInput.cs
Okay, here's the implementation.

    public void Apply(IPongGame game)
    {
        foreach (var playerSlot in game.PlayerSlots)
        {
            if (keyboard.IsPressed(playerSlot.StartKey))
            {
                game.Join(playerSlot);
            }
        }
    }

We could've done another cheesy implementation like this:

    public void Apply(IPongGame game)
    {
        if (keyboard.IsPressed(game.PlayerSlots[0].StartKey))
        {
            game.Join(game.PlayerSlots[0]);
        }
    }

But I think we see where this is going. However, since we've taken this step, it's a good idea to add a test that ensures that we're checking both player slots. Otherwise, player two's join mechanism could be horribly broken and our tests wouldn't catch it.

## ef137b

### PongInputTest.cs
And this is that test. Very similar to the first one, which means it's probably time to consider abstracting away some of the similarities.

    [Test]
    public void Calls_Join_when_both_players_press_start()
    {
        var playerSlot1 = Stub<IPlayerSlot>();
        playerSlot1.Setup(p => p.StartKey).Returns(Key.Enter);
        var playerSlot2 = Stub<IPlayerSlot>();
        playerSlot2.Setup(p => p.StartKey).Returns(Key.Tab);

        Keyboard.Setup(k => k.IsPressed(Key.Enter)).Returns(true);
        Keyboard.Setup(k => k.IsPressed(Key.Tab)).Returns(true);

        var game = Mock<IPongGame>();
        game.Setup(g => g.PlayerSlots).Returns(new IPlayerSlot[] { playerSlot1.Object, playerSlot2.Object });
        game.Setup(g => g.Join(playerSlot1.Object));
        game.Setup(g => g.Join(playerSlot2.Object));

        Input.Apply(game.Object);
    }

## e04837

### PongInputTest.cs
Excellent, now our pair of tests are substantially more concise.

    [SetUp]
    public void SetUp()
    {
        Keyboard = Stub<IKeyboardInput>();
        Input = new PongInput(Keyboard.Object);
        PlayerSlot1 = Stub<IPlayerSlot>();
        PlayerSlot1.Setup(p => p.StartKey).Returns(Key.Enter);
        PlayerSlot2 = Stub<IPlayerSlot>();
        PlayerSlot2.Setup(p => p.StartKey).Returns(Key.Tab);
        Game = Mock<IPongGame>();
        Game.Setup(g => g.PlayerSlots).Returns(new IPlayerSlot[] { PlayerSlot1.Object, PlayerSlot2.Object });
    }

    public void KeyIsPressed(Key key)
    {
        Keyboard.Setup(k => k.IsPressed(key)).Returns(true);
    }

    public void KeyIsNotPressed(Key key)
    {
        Keyboard.Setup(k => k.IsPressed(key)).Returns(false);
    }

    [Test]
    public void Calls_Join_when_player_presses_start()
    {
        KeyIsPressed(Key.Enter);
        KeyIsNotPressed(Key.Tab);

        Game.Setup(g => g.Join(PlayerSlot1.Object));

        Input.Apply(Game.Object);
    }

    [Test]
    public void Calls_Join_when_both_players_press_start()
    {
        KeyIsPressed(Key.Enter);
        KeyIsPressed(Key.Tab);

        Game.Setup(g => g.Join(PlayerSlot1.Object));
        Game.Setup(g => g.Join(PlayerSlot2.Object));

        Input.Apply(Game.Object);
    }

## b051fa
Okay, it's time to hook up the rest of this shit. I'm gonna go faster and into a little less detail now.

This commit rounds out our initial pass at input and display. You'll notice that I added a lot of untested code, but it's all contained in KeyboardInput and PongDisplay -- two classes that are not eminently testable at the moment.

PongDisplay should probably have some tests right now, simply due to the fact that it displays "Press <key> to Join" if the slot is open or "Ready" if it is taken. This is behavior that is important to the user, and we'd like to catch it if it breaks some day.

### Side-note
Allegro 5.0.4 changed the API of several methods since 5.0 (or maybe 4.9?), which caused my C# binding to shit its pants. (Segfaults, stack corruption, etc.) That was really annoying to deal with, but I don't hold it against the Allegro devs at all.

## dfb4c9a
I went back and tested PongDisplay. There are a lot of tests in this commit, so go check them out. I'll point out the most interesting changes below.

### PlayerSlot.cs
In the process of testing PongDisplay, I took out the magic values and attached them to PlayerSlots.

    public class PlayerSlot : IPlayerSlot
    {
        public Key StartKey
        {
            get;
            set;
        }
        public Color Color
        {
            get;
            set;
        }
        public Point JoinReadyPosition
        {
            get;
            set;
        }
        public FontDrawFlags JoinReadyFontDrawFlags
        {
            get;
            set;
        }
        public bool IsReady
        {
            get { return Player != null; }
        }
        public IPlayer Player
        {
            get;
            private set;
        }
        public string ReadyText
        {
            get { return "Ready"; }
        }
        public string JoinText
        {
            get { return String.Format("Press {0} to Join", StartKey); }
        }
        public void Join(IPlayer player)
        {
            Player = player;
        }
    }

### PlayerSlotRenderer.cs
I made a PlayerSlotRenderer to handle the logic flow of whether to show "Press <key> to Join" or "Ready". When I did, I realized that asking PongGame whether a slot was ready seemed stupid because it didn't need PongGame for anything else, so now the "readiness" is accessed from the PlayerSlot directly instead.

    public class PlayerSlotRenderer : IPlayerSlotRenderer
    {
        public PlayerSlotRenderer(IFontRenderer fontRenderer)
        {
            this.fontRenderer = fontRenderer;
        }

        private IFontRenderer fontRenderer;

        public void Render(IPlayerSlot playerSlot)
        {
            fontRenderer.Render(playerSlot.Color, playerSlot.JoinReadyPosition, playerSlot.JoinReadyFontDrawFlags, playerSlot.IsReady ? playerSlot.ReadyText : playerSlot.JoinText);
        }
    }

### IFontRenderer.cs, FontRenderer.cs
I introduced a FontRenderer object, which is pretty nice because its interface IFontRenderer doesn't talk about Font objects directly -- objects which require loading from a file, something we don't want to rely on for simple test data. (Missing a font file is a really stupid reason for a test to fail!)

    public interface IFontRenderer
    {
        void Render(Color color, Point position, FontDrawFlags flags, string text);
    }

    public class FontRenderer : IFontRenderer
    {
        public Font Font
        {
            get;
            set;
        }

        public void Render(Color color, Point position, FontDrawFlags flags, string text)
        {
            Font.Draw(color, position.X, position.Y, flags, text);
        }
    }

### Content.cs
I also made a static Content class, which encapsulates all of the static data (such as fonts) that can only be loaded at runtime, after Allegro has been fully initialized. Because of .NET static initialization rules, the static data will be loaded the first time that the Content class is referenced.

    /// <summary>
    /// Any expression that makes a reference to this type can potentially throw an exception by being the first to
    /// trigger the static initializers. This is only safe to do /after/ Allegro has been fully initialized.
    /// </summary>
    public static class Content
    {
        public static Font Arial = LoadFont("Arial.ttf", 14, TtfFlags.None);

        public static Font LoadFont(string filename, int size, TtfFlags flags)
        {
            var font = Ttf.LoadFont(filename, size, flags);
            if (font == null)
            {
                throw new Exception(String.Format("LoadFont({0}, {1}, {2}) Failed", filename, size, flags));
            }
            return font;
        }
    }

### Main.cs
Main has become just a place to stage the big-bang explosion of classes, mixed with their configuration data. This is the kind of stuff that dependency injection frameworks do for you, but it's alright to do it by hand too.

    class MainClass
    {
        public static void Main(string[] args)
        {
            Allegro.RunMain(AllegroMain);
        }

        public static void AllegroMain()
        {
            InitializeAllegro();

            var game = CreateGame();
            var display = CreateDisplay();
            var input = CreateInput();

            while (game.IsRunning)
            {
                input.Apply(game);
                //game.Update(10);
                display.Render(game);
            }
        }

        public static void InitializeAllegro()
        {
            if (!Allegro.InstallSystem())
            {
                throw new Exception("allegro failz");
            }
            if (!Image.Init())
            {
                throw new Exception("image failz");
            }
            Font.Init();
            if (!Ttf.Init())
            {
                throw new Exception("ttf failz");
            }
            Display.Create(800, 600);
        }

        public static IPongGame CreateGame()
        {
            return new PongGame
            {
                PlayerSlots = new IPlayerSlot[] {
                    new PlayerSlot
                    {
                        Color = new Color(1, 0, 0),
                        JoinReadyPosition = new Point(100, 50),
                        JoinReadyFontDrawFlags = FontDrawFlags.AlignLeft,
                        StartKey = Key.Num1,
                    },
                    new PlayerSlot
                    {
                        Color = new Color(0, 0, 1),
                        JoinReadyPosition = new Point(Display.Current.Width - 100, 50),
                        JoinReadyFontDrawFlags = FontDrawFlags.AlignRight,
                        StartKey = Key.Num0
                    }
                }
            };
        }

        public static IPongDisplay CreateDisplay()
        {
            return new PongDisplay(
                new Renderer(),
                new PlayerSlotRenderer(
                    new FontRenderer
                    {
                        Font = Content.Arial
                    }
                )
            );
        }

        public static IPongInput CreateInput()
        {
            return new PongInput(
                new KeyboardInput()
            );
        }
    }

### Git at it!
There's getting to be too much code to show at every step! It's interesting to see all the old, shitty code replaced by more modular, testable code, so I recommend checking out a diff of the commit through GitX or something.

## 649c51
Paddles now spawn at the player's assigned spawn point. Nothing especially exciting.

## 8dc59da
Ah, I found a bug! If you hold down the start key, it is interpreted as joining multiple times, which causes the paddle to be continually nullified. After catching this bug, I wrote a test case to demonstrate the erroneous behavior and serve as a regression test to make sure the bug stays squashed for good.

### JoinTest.cs

    [Test]
    public void Joining_multiple_times_is_okay()
    {
        Game.Join(Game.PlayerSlots[0]);
        Game.Join(Game.PlayerSlots[1]);
        Game.Join(Game.PlayerSlots[1]);
        Assert.That(Game.Players[1].Paddle, Is.Not.Null);
    }

### PongGameTest.cs

    [Test]
    public void Ignores_joining_occupied_slot()
    {
        var playerSlot = Mock<IPlayerSlot>();
        playerSlot.Setup(p => p.IsReady).Returns(true);
        Game.Join(playerSlot.Object);
    }

### PongGame.cs

    public void Join(IPlayerSlot playerSlot)
    {
        if (playerSlot.IsReady)
        {
            return;
        }
        …

## 29e672
I added ball spawning and initialization, as well as rendering for paddles and the ball.

A couple of interesting things to note:

### Factories
In general, I only allow myself to **new** up stateful objects when I'm in a factory. Stateful objects are those that express different behavior depending on their internal state -- they are not dumb service objects, and they are not merely containers for data.

First of all, if I have a method that is responding to the game starting by doing a number of things, among them creating a Ball and put it in the right state, then knowing *which* kind of Ball to create and *how* to put it in the right state is not part of its job.

Second, stateful objects by their very nature must be replaced by mocks in tests but I can't replace an object with a mock if you already said to use the real thing.

    [Test]
    public void Game_creates_ball()
    {
        var createCall = BallFactory.Setup(b => b.Create(new Point(40, 70))); // Middle of the screen
        createCall.Returns(Ball);
        createCall.Verifiable();
        Start(Game);
    }

### Randomness
Randomness is one of those interesting testing problems, because it's impossible to reliably test something with pseudorandom behavior. Taking a statistical approach is foolish, because not only is it often more trouble than its worth, it also introduces the possibility of random test failures, which are a good way to lose confidence in your test suite and end up pursuing false positives and negatives.

My preferred approach is to isolate the randomness, mock it out in unit tests, and use a deterministic version in functional/integration tests. Production code just slips in the real thing and everything else is none the wiser.

    public class RiggedBallInitializer : IBallInitializer
    {
        public Point SpawnVelocity { get; set; }

        public void Initialize(IBall ball)
        {
            ball.Velocity = SpawnVelocity;
        }
    }

    public class RandomBallInitializer : IBallInitializer
    {
        private Random random = new Random();

        public void Initialize(IBall ball)
        {
            ball.Velocity = new Point(random.Next(-10, 10), random.Next(-10, 10));
        }
    }