using Mirror;
using UnityEngine;

namespace UI
{
	public class IndicatorController : MonoBehaviour
	{
		[Header("SETTINGS:")]
		[SerializeField] private float outOfSightOffset = 50f;
		
		[Header("REFERENCES:")]
		[SerializeField] private GameObject offScreenIndicator;
		[SerializeField] private RectTransform indicatorTransform;
		[SerializeField] private RectTransform arrowTransform;
		[SerializeField] private RectTransform canvasRect;

		[Header("DEBUG:")] 
		public Camera mainCamera;
		public GameObject mapFlag;
		public GameObject target;

		public bool localPlayerHasFlag;

		private void Start()
		{
			// TODO? - Improved setting of external references
			mapFlag = GameObject.Find("Flag");
			mainCamera = Camera.main;
			target = mapFlag;
		}

		public void Update()
	    {
		    if (localPlayerHasFlag)
			    return;
		    
	        SetIndicatorPosition();
	        //Adjust distance display?
	    }

		private void SetIndicatorPosition()
	    {
		    //Get the position of the target in relation to the screenSpace 
		    Vector3 indicatorPosition = mainCamera.WorldToScreenPoint(target.transform.position);

		    if (indicatorPosition.z < 0)
		    {
			    indicatorPosition.x *= -1;
			    indicatorPosition.y *= -1;
		    }

		    //In case the target is both in front of the camera and within the bounds of its frustrum
	        if (indicatorPosition.z >= 0f & indicatorPosition.x <= canvasRect.rect.width * canvasRect.localScale.x
	         & indicatorPosition.y <= canvasRect.rect.height * canvasRect.localScale.x & indicatorPosition.x >= 0f & indicatorPosition.y >= 0f)
	        {
		        //Target is in sight, change indicator parts around accordingly
	            TargetOutOfSight(false, indicatorPosition);
	        }

	        //In case the target is out of sight
	        else
	        {
	            //Set indicatorposition and set targetIndicator to outOfSight form.
	            indicatorPosition = OutOfRangeIndicatorPositionB(indicatorPosition);
	            TargetOutOfSight(true, indicatorPosition);
	        }
	        
	        indicatorTransform.position = indicatorPosition;
	    }

	    private Vector3 OutOfRangeIndicatorPositionB(Vector3 indicatorPosition)
	    {
	        //Set indicatorPosition.z to 0f
	        indicatorPosition.z = 0f;

	        Rect rect = canvasRect.rect;
	        
	        //Calculate Center of Canvas and subtract from the indicator position to have indicatorCoordinates from the Canvas Center instead the bottom left!
	        Vector3 canvasCenter = new Vector3(rect.width * 0.5f, rect.height * 0.5f, 0f) * canvasRect.localScale.x;
	        indicatorPosition -= canvasCenter;

	        //Calculate if Vector to target intersects (first) with y border of canvas rect or if Vector intersects (first) with x border:
	        //This is required to see which border needs to be set to the max value and at which border the indicator needs to be moved (up & down or left & right)
	        float divX = (rect.width * 0.5f - outOfSightOffset) / Mathf.Abs(indicatorPosition.x);
	        float divY = (canvasRect.rect.height * 0.5f - outOfSightOffset) / Mathf.Abs(indicatorPosition.y);

	        //In case it intersects with x border first, put the x-one to the border and adjust the y-one accordingly (Trigonometry)
	        if (divX < divY)
	        {
	            float angle = Vector3.SignedAngle(Vector3.right, indicatorPosition, Vector3.forward);
	            indicatorPosition.x = Mathf.Sign(indicatorPosition.x) * (canvasRect.rect.width * 0.5f - outOfSightOffset) * canvasRect.localScale.x;
	            // indicatorPosition.x = Mathf.Sign(indicatorPosition.x) * (canvasRect.rect.width * 0.5f) * canvasRect.localScale.x;
	            indicatorPosition.y = Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.x;
	        }
	        //In case it intersects with y border first, put the y-one to the border and adjust the x-one accordingly (Trigonometry)
	        else
	        {
	            float angle = Vector3.SignedAngle(Vector3.up, indicatorPosition, Vector3.forward);
	            indicatorPosition.y = Mathf.Sign(indicatorPosition.y) * (canvasRect.rect.height * 0.5f - outOfSightOffset) * canvasRect.localScale.y;
	            // indicatorPosition.y = Mathf.Sign(indicatorPosition.y) * (canvasRect.rect.height * 0.5f) * canvasRect.localScale.y;
	            indicatorPosition.x = -Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.y;
	        }
	        //Change the indicator Position back to the actual rectTransform coordinate system and return indicatorPosition
	        indicatorPosition += canvasCenter;
	        
	        return indicatorPosition;
	    }
	    
	    private void TargetOutOfSight(bool outOfSight, Vector3 indicatorPosition)
	    {
		    if (outOfSight)
	        {
		        if (offScreenIndicator.gameObject.activeSelf == false)
	            {
		            offScreenIndicator.gameObject.SetActive(true);
	            }

	            //Set the rotation of the OutOfSight-Arrow
		        arrowTransform.rotation = Quaternion.Euler(RotationOutOfSightIndicator(arrowTransform.position));
	        }
	        else if (offScreenIndicator.gameObject.activeSelf)
	        {
		        offScreenIndicator.gameObject.SetActive(false);
	        }
	    }
	    
	    private Vector3 RotationOutOfSightIndicator(Vector3 indicatorPosition)
	    {
		    Rect rect = canvasRect.rect;
		    
		    //Calculate the canvasCenter
		    Vector3 canvasCenter = new Vector3(rect.width * 0.5f, rect.height * 0.5f, 0f) * canvasRect.localScale.x;

	        //Calculate the signedAngle between the position of the indicator and the Direction up.
	        float angle = Vector3.SignedAngle(Vector3.up, indicatorPosition - canvasCenter, Vector3.forward);

	        //return the angle as a rotation Vector
	        return new Vector3(0f, 0f, angle + 180);
	    }

	    public void SetTarget(bool targetIsAPlayer, GameObject newTarget = null)
	    {
		    if (targetIsAPlayer && newTarget != null)
		    {
			    target = newTarget;
			    
			    if (newTarget.GetComponent<NetworkBehaviour>().isLocalPlayer)
			    {
				    localPlayerHasFlag = true;
			    }
		    }
		    else
		    {
			    target = mapFlag;
			    localPlayerHasFlag = false;
		    }
	    }
	}
}