using UnityEngine;

public class HitPopcornGenerator : MonoBehaviour
{
    [SerializeField] private HitPopcorn _popcornPrefab;

    private static HitPopcornGenerator _popcornGenerator;

    public static HitPopcornGenerator Instance =>
        _popcornGenerator;

    private void Start()
    {
        _popcornGenerator = this;
    }

    public void GeneratePopcorn(Vector3 worldPosition, Color color, int damage)
    {
        var newPopcorn = Instantiate(_popcornPrefab);

        newPopcorn.RectTransform.parent = transform;
        newPopcorn.WorldPosition = worldPosition;
        newPopcorn.UpdatePosition(0);
        newPopcorn.Text.text = damage.ToString();
        newPopcorn.Text.color = color;
        
        
    }
}
