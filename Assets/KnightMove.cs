using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class KnightMove : PieceMove
{
    void Start()
    {
        base.Start();
    }

    public override void GenMovableGrid()
    {
        var possibleAxis = new List<Tuple<float, float>>();
        var curPos = transform.localPosition;
        if (calculator.CheckValidAxis(curPos.x + 2 * calculator.xScaleUnit, curPos.y + 4 * calculator.yScaleUnit, color))
            possibleAxis.Add(new Tuple<float, float>(curPos.x + 2 * calculator.xScaleUnit, curPos.y + 4 * calculator.yScaleUnit));

        if (calculator.CheckValidAxis(curPos.x + 2 * calculator.xScaleUnit, curPos.y - 4 * calculator.yScaleUnit, color))
            possibleAxis.Add(new Tuple<float, float>(curPos.x + 2 * calculator.xScaleUnit, curPos.y - 4 * calculator.yScaleUnit));

        if (calculator.CheckValidAxis(curPos.x - 2 * calculator.xScaleUnit, curPos.y + 4 * calculator.yScaleUnit, color))
            possibleAxis.Add(new Tuple<float, float>(curPos.x - 2 * calculator.xScaleUnit, curPos.y + 4 * calculator.yScaleUnit));

        if (calculator.CheckValidAxis(curPos.x - 2 * calculator.xScaleUnit, curPos.y - 4 * calculator.yScaleUnit, color))
            possibleAxis.Add(new Tuple<float, float>(curPos.x - 2 * calculator.xScaleUnit, curPos.y - 4 * calculator.yScaleUnit));

        if (calculator.CheckValidAxis(curPos.x + 4 * calculator.xScaleUnit, curPos.y + 2 * calculator.yScaleUnit, color))
            possibleAxis.Add(new Tuple<float, float>(curPos.x + 4 * calculator.xScaleUnit, curPos.y + 2 * calculator.yScaleUnit));

        if (calculator.CheckValidAxis(curPos.x + 4 * calculator.xScaleUnit, curPos.y - 2 * calculator.yScaleUnit, color))
            possibleAxis.Add(new Tuple<float, float>(curPos.x + 4 * calculator.xScaleUnit, curPos.y - 2 * calculator.yScaleUnit));

        if (calculator.CheckValidAxis(curPos.x - 4 * calculator.xScaleUnit, curPos.y + 2 * calculator.yScaleUnit, color))
            possibleAxis.Add(new Tuple<float, float>(curPos.x - 4 * calculator.xScaleUnit, curPos.y + 2 * calculator.yScaleUnit));

        if (calculator.CheckValidAxis(curPos.x - 4 * calculator.xScaleUnit, curPos.y - 2 * calculator.yScaleUnit, color))
            possibleAxis.Add(new Tuple<float, float>(curPos.x - 4 * calculator.xScaleUnit, curPos.y - 2 * calculator.yScaleUnit));

        if (possibleAxis.Count > 0)
            InstantiateTiles(possibleAxis);
    }
}
