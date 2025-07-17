
using UnityEngine;
using UnityEngine.Tilemaps;
[CreateAssetMenu]
public class ruleTileCombine : RuleTile<ruleTileCombine.Neighbor> {
    public class Neighbor : RuleTile.TilingRule.Neighbor {}
    public override bool RuleMatch(int neighbor, TileBase tile) {
        if(neighbor==1){
            return tile!=null;
        }
        return base.RuleMatch(neighbor, tile);
    }
}