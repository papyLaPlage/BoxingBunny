using UnityEngine;

//Positions 2D 
public class Point2D
{
	public EnumCameraPlan enumCameraPlan;
	public Vector2 position = Vector2.zero;
	public Vector2 decalage = Vector2.zero;
	private Vector2 cameraPosition;

	public Point2D()
	{
	}

	public Point2D(float X, float Y)
	{
		position = new Vector2(X, Y);
	}

	public Point2D(float X, float Y, float DX, float DY)
	{
		position = new Vector2(X, Y);
		decalage = new Vector2(DX, DY);
	}

	public Point2D(Vector2 _position)
	{
		position = _position;
	}

	public Point2D(Vector2 _position, Vector2 _decalage)
	{
		position = _position;
		decalage = _decalage;
	}

	public Point2D(Vector3 _position)
	{
		position = _position;
	}

	public Point2D(Vector3 _position, Vector3 _decalage)
	{
		position = _position;
		decalage = _decalage;
	}

	public Vector2 CameraPosition
	{
		get
		{
			return position + decalage;
		}
	}

	public static Point2D zero
	{
		get
		{
			return new Point2D();
		}
	}

}
