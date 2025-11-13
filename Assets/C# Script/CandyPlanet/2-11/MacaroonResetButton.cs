using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MacaroonResetButton : MonoBehaviour
{
    public void OnClickReset()
    {
        Macaron[] macarons = FindObjectsOfType<Macaron>();
        MacaroonPlate plate = FindObjectOfType<MacaroonPlate>();

        // 접시 리스트 초기화
        if (plate != null)
        {
            var field = typeof(MacaroonPlate).GetField("stackedMacarons",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var list = field.GetValue(plate) as IList<Macaron>;
                list?.Clear();
            }
        }

        // 마카롱들을 원래 위치로 되돌리기
        foreach (var macaron in macarons)
        {
            macaron.transform.SetParent(null);
            macaron.MoveTo(macaron.transform.position = macaron.GetOriginalPosition());

            var drag = macaron.GetComponent<DragAndDrop>();
            if (drag != null) drag.banDragging = false;

            var sr = macaron.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 0;

            // isStacked false로 변경
            var field = typeof(Macaron).GetField("isStacked",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(macaron, false);
        }
    }
}
