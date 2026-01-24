using UnityEngine;
using UnityEngine.SceneManagement;

public class InputHandler : MonoBehaviour // <--- Added this!
{
    [SerializeField] // <--- Fixed spelling (was SerializedField)
    CarHandler carHandler;

    void Update()
    {
        Vector2 input = Vector2.zero;

        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");

        carHandler.SetInput(input);

        // Fixed: Input.GetKeyDown is a static function, not part of your 'input' vector
        if(Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}