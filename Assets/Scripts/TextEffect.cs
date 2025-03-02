using UnityEngine;
using TMPro;

public class TextEffect : MonoBehaviour
{
    public static void UpdateText(TMP_Text text, Vector2 speed, Vector2 amplitude)
    {
        text.ForceMeshUpdate();
        var info = text.textInfo;

        for (int i = 0; i < info.characterCount; i++)
        {
            var ch = info.characterInfo[i];
            if (!ch.isVisible) continue;

            var vert = info.meshInfo[ch.materialReferenceIndex].vertices;

            for (int j = 0; j < 4; ++j)
            {
                int index = j + ch.vertexIndex;
                Vector3 origin = vert[index];
                vert[index] = origin + new Vector3(Mathf.Cos((Time.unscaledTime * speed.x) + i) * amplitude.x, Mathf.Sin((Time.unscaledTime * speed.y) + i) * amplitude.y, 0);
            }
        }

        for (int i = 0; i < info.meshInfo.Length; ++i)
        {
            var meshInfo = info.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            text.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
