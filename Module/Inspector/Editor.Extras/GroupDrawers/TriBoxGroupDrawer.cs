﻿using VirtueSky.Inspector;
using VirtueSky.Inspector.Elements;
using VirtueSky.Inspector.GroupDrawers;

[assembly: RegisterTriGroupDrawer(typeof(TriBoxGroupDrawer))]

namespace VirtueSky.Inspector.GroupDrawers
{
    public class TriBoxGroupDrawer : TriGroupDrawer<DeclareBoxGroupAttribute>
    {
        public override TriPropertyCollectionBaseElement CreateElement(DeclareBoxGroupAttribute attribute)
        {
            return new TriBoxGroupElement(new TriBoxGroupElement.Props
            {
                title = attribute.Title,
                titleMode = attribute.HideTitle
                    ? TriBoxGroupElement.TitleMode.Hidden
                    : TriBoxGroupElement.TitleMode.Normal,
                hideIfChildrenInvisible = true,
            });
        }
    }
}