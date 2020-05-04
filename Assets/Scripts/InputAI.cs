using UnityEngine;


public class InputAI : MonoBehaviour {

    MovementAI movementAI;

    void Start() {
        movementAI = GetComponent<MovementAI>();
    }

    public void InputUpdateAI(Vector2 directionalInput) {
        //Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        movementAI.SetDirectionalInput(directionalInput);

        //if (Input.GetButtonDown("Jump")) movementAI.OnJumpInputDown();

        //if (Input.GetButtonUp("Jump")) movementAI.OnJumpInputUp();

        //if (Input.GetButtonDown("SideDash")) movementAI.OnRollInput();

        //if (Input.GetButtonDown("Hit")) movementAI.OnHitInput();
    }
}
