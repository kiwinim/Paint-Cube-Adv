using UnityEngine;
using UnityEngine.U2D;

public class SoftBody : MonoBehaviour
{
    #region Constants
    private const float splineOffset = 0.5f;
    #endregion

    #region Fields
    [SerializeField]
    public SpriteShapeController spriteShape;
    public SpriteShapeController inspriteShape;
    [SerializeField]
    public Transform[] points;
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        UpdateVerticies();
    }

    private void Update()
    {
        UpdateVerticies();
    }
    #endregion

    #region privateMethods
    private void UpdateVerticies()
    {
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 _vertex = points[i].localPosition;

            Vector2 _towardsCenter = (Vector2.zero - _vertex).normalized;

            float _colliderRadius = points[i].gameObject.GetComponent<CircleCollider2D>().radius;
            try
            {
                spriteShape.spline.SetPosition(i, (_vertex - _towardsCenter * _colliderRadius));
                inspriteShape.spline.SetPosition(i, (_vertex - _towardsCenter * _colliderRadius));
            }
            catch
            {
                Debug.Log("Spline points are too close to each other.. recalculate");
                spriteShape.spline.SetPosition(i, (_vertex - _towardsCenter * (_colliderRadius + splineOffset)));
                inspriteShape.spline.SetPosition(i, (_vertex - _towardsCenter * (_colliderRadius + splineOffset)));
            }

            Vector2 _lt = inspriteShape.spline.GetLeftTangent(i);
             _lt = spriteShape.spline.GetLeftTangent(i);

            Vector2 _newRt = Vector2.Perpendicular(_towardsCenter) * _lt.magnitude;
            Vector2 _newLt = Vector2.zero - (_newRt);

            spriteShape.spline.SetRightTangent(i, _newRt);
            spriteShape.spline.SetLeftTangent(i, _newLt);
            inspriteShape.spline.SetRightTangent(i, _newRt);
            inspriteShape.spline.SetLeftTangent(i, _newLt);
        }
    }
    #endregion
}
