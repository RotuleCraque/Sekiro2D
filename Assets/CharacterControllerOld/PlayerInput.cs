using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour {

    Player player;

    void Start() {
        player = GetComponent<Player>();
    }

    public void PlayerInputUpdate() {
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.SetDirectionalInput(directionalInput);

        if (Input.GetButtonDown("Jump")) player.OnJumpInputDown();

        if (Input.GetButtonUp("Jump")) player.OnJumpInputUp();

        if (Input.GetButtonDown("SideDash")) player.OnRollInput();

        if (Input.GetButtonDown("Hit")) player.OnHitInput();
    }
}
