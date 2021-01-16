using System;
using System.Linq;
using UnityEngine;

namespace Piglet
{
	/// <summary>
	/// Utility methods for reading/loading textures.
	/// </summary>
	public class TextureUtil
	{
		/// <summary>
		/// Flip a texture upside down. This operation is needed
		/// because `Texture.LoadImage` imports .png/.jpg images
		/// into textures upside down (I don't know why).
		///
		/// Note that this method does not work reliably in
		/// WebGL builds. In particular, it was generating black textures
		/// in Chrome 79.0.3945.79 (64-bit).  It seems that
		/// the `Graphics.Blit` operation is not working as intended
		/// or is not completing before the `Texture2D.ReadPixels`
		/// operation is performed, and I was never able to figure
		/// out why.
		/// </summary>
		/// <param name="texture"></param>
		/// <returns></returns>
		public static Texture2D FlipTexture(Texture2D texture)
		{
			Material flippedMaterial = new Material(
				Shader.Find("Piglet/FlipTexture"));

			var flippedTexture = new Texture2D(
				texture.width, texture.height,
				texture.format, texture.mipmapCount > 1);
			flippedTexture.name = texture.name;

			var renderTexture = RenderTexture.GetTemporary(
				texture.width, texture.height, 32,
				RenderTextureFormat.ARGB32);

			Graphics.Blit(texture, renderTexture, flippedMaterial);

			RenderTexture prevActive = RenderTexture.active;
			RenderTexture.active = renderTexture;

			flippedTexture.ReadPixels(
				new Rect(0, 0, texture.width, texture.height),
				0, 0, texture.mipmapCount > 1);
			flippedTexture.Apply();

			RenderTexture.active = prevActive;
			RenderTexture.ReleaseTemporary(renderTexture);

			return flippedTexture;
		}
	}
}