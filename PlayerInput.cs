// Copyright (c) 2021 RogueWare

using System;
using Mirror;
using UnityEngine;

// This class should be used to handle any input from the player no matter what they are controlling
// If in the future we add things for example, vehicles the vehicle should subscribe to input events
// broadcast by this class
public class PlayerInput : NetworkBehaviour
{
	// used to pass in two floats
	public class EventArgs2D : EventArgs
	{
		private readonly float x;
		private readonly float y;

		public EventArgs2D(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public float GetX()
		{
			return x;
		}

		public float GetY()
		{
			return y;
		}
	}

	// delegates and events
	public delegate void FirePressedEventHandler(EventArgs2D args);
	public delegate void FireReleasedEventHandler(EventArgs args);
	public delegate void MouseMoveEventHandler(EventArgs2D args);
	public delegate void PlayerMoveEventHandler(EventArgs2D args);
	public delegate void InventoryCycleEventHandler(EventArgs2D args);
	public delegate void PlayerStopEventHandler();
	public event FirePressedEventHandler FirePressed;
	
	public event FireReleasedEventHandler FireReleased;
	
	public event MouseMoveEventHandler MouseMove;
	public event PlayerMoveEventHandler PlayerMove;
	public event InventoryCycleEventHandler InventoryCycle;
	public event PlayerStopEventHandler PlayerStopMoving;

	// input state variables
	private float horizontalMovement = 0f;
	private float verticalMovement = 0f;
	private float mouseDeltaX = 0f;
	private float mouseDeltaY = 0f;
	private float scrollDelta = 0f;
	private bool fireButtonHeld = false;
	private bool cycleKey = false;
	private bool wasMoving = false;
	private bool fireButtonReleased = false; // Only true if fire button was pressed in previous frame
	private bool fireButtonPressed = false; // Only true on the first frame fire button is pressed
	private bool prevFramFireButtonHeld = false;

	private bool gameOver = false;

	public override void OnStartAuthority()
	{
		// confine cursor to the game window
		Cursor.lockState = CursorLockMode.Confined;
		Cursor.visible = false;
	}

	// update input state variables every rendered frame
	private void Update()
	{
		if(!isLocalPlayer || gameOver)
		{
			return;
		}

		horizontalMovement = Input.GetAxisRaw("HorizontalMovement");
		verticalMovement = Input.GetAxisRaw("VerticalMovement");
		mouseDeltaX = Input.GetAxisRaw("Mouse X");
		mouseDeltaY = Input.GetAxisRaw("Mouse Y");
		scrollDelta = Input.GetAxisRaw("ScrollWheelInventoryCycle");
		cycleKey = Input.GetButtonDown("KeyInventoryCycle");
		fireButtonHeld = Input.GetButton("Fire");
		if(fireButtonHeld && fireButtonHeld != prevFramFireButtonHeld)
		{
			fireButtonPressed = true;
		}

		// broadcast mouse movement every frame
		if(mouseDeltaX != 0f || mouseDeltaY != 0f)
		{
			// broadcast mouse move event
			OnMouseMove(mouseDeltaX, mouseDeltaY, Time.fixedDeltaTime);
		}

		if(fireButtonHeld)
		{
			fireButtonReleased = false;
		}

		if(fireButtonPressed)
		{
			// broadcast fire event
			OnFirePressed(Input.mousePosition.x, Input.mousePosition.y);
			fireButtonPressed = false;
		}

		if(fireButtonReleased)
		{
			// broadcast fire release
			OnFireReleased();
		}

		// if there is any kind of scrolling
		if(scrollDelta != 0 || cycleKey)
		{
			if(cycleKey)
			{
				scrollDelta = 1f; // make scrollDelta a positive value when calling OnInventoryCycle() with key press
			}

			OnInventoryCycle();
		}

		fireButtonReleased = fireButtonHeld;
		prevFramFireButtonHeld = fireButtonHeld;
	}

	// FixedUpdate runs at a consistent tick rate independent of the frame-rate of the game.
	// Runs at the frequency of the physics engine. Definitely want player movement in here so that their velocity
	// does not depend on the framerate of the game.
	[ClientCallback]
	private void FixedUpdate()
	{
		if(!isLocalPlayer)
		{
			return;
		}

		if(horizontalMovement != 0f || verticalMovement != 0f)
		{
			// broadcast player movement event
			wasMoving = true;
			OnPlayerMove(horizontalMovement, verticalMovement, Time.fixedDeltaTime);
		}
		else if(wasMoving)
		{
			OnPlayerStop();
			wasMoving = false;
		}
	}

	private void OnMouseMove(float dx, float dy, float dt)
	{
		if(MouseMove != null)
		{
			MouseMove(new EventArgs2D(dx * dt, dy * dt));
		}
	}

	private void OnPlayerMove(float dx, float dy, float dt)
	{
		if(PlayerMove != null)
		{
			PlayerMove(new EventArgs2D(dx, dy));
		}
	}

	private void OnPlayerStop()
	{
		if(PlayerStopMoving != null)
		{
			PlayerStopMoving();
		}
	}

	private void OnFirePressed(float mouseX, float mouseY)
	{
		if(FirePressed != null)
		{
			FirePressed(new EventArgs2D(mouseX, mouseY));
		}
	}

	private void OnFireReleased()
	{
		if(FireReleased != null)
		{
			FireReleased(EventArgs.Empty);
		}
	}


	private void OnInventoryCycle()
	{
		if(InventoryCycle != null)
		{
			InventoryCycle(new EventArgs2D(scrollDelta, 0f));
		}
	}

	public void GameOver()
	{
		Cursor.visible = true;
		gameOver = true;
	}

	public void EnableInput()
	{
		gameOver = false;
		Cursor.visible = false;
	}
}
