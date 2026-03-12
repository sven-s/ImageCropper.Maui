using System;
using ObjCRuntime;

namespace Bind_TOCropViewController
{
    [Native]
    public enum TOCropViewCroppingStyle : long
    {
        Default,
        Circular
    }

    [Native]
    public enum TOCropViewControllerToolbarPosition : long
    {
        Bottom,
        Top
    }
}
