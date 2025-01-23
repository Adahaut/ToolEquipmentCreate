using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace RectCaca
{
    public static class RectUtility
    {
        public static Rect SetX(this Rect rect, float x)
        {
            rect.x = x;
            return rect;
        }

        public static Rect SetY(this Rect rect, float y)
        {
            rect.y = y;
            return rect;
        }

        public static Rect SetWidth(this Rect rect, float width)
        {
            rect.width = width;
            return rect;
        }

        public static Rect SetHeight(this Rect rect, float height)
        {
            rect.height = height;
            return rect;
        }

        public static Rect MoveX(this Rect rect, float x)
        {
            rect.x += x;
            return rect;
        }

        public static Rect MoveY(this Rect rect, float y)
        {
            rect.y += y;
            return rect;
        }

        public static Rect Grow(this Rect rect, float width, float height)
        {
            rect.width += width;
            rect.height += height;
            return rect;
        }

        public static Rect Shrink(this Rect rect, float width, float height)
        {
            rect.width -= width;
            rect.height -= height;
            return rect;
        }

        public static Rect SliceH(this Rect rect, float percent)
        {
            CheckPercent(percent);
            rect.y = rect.height * (1 - percent);
            rect.height *= percent;
            return rect;
        }

        public static Rect SliceV(this Rect rect, float percent)
        {
            CheckPercent(percent);
            rect.x = rect.width * (1 - percent);
            rect.width *= percent;
            return rect;
        }

        public static Rect ReminderH(this Rect rect, float percent)
        {
            CheckPercent(percent);
            rect.y = rect.height * percent;
            rect.height *= 1 - percent;
            return rect;
        }

        public static Rect ReminderV(this Rect rect, float percent)
        {
            CheckPercent(percent);
            rect.x = rect.width * percent;
            rect.width *= 1 - percent;
            return rect;
        }

        public static Rect RightHalf(this Rect rect)
        {
            rect = ReminderV(rect, 0.5f);
            return rect;
        }

        public static Rect LeftHalf(this Rect rect)
        {
            rect = SliceV(rect, 0.5f);
            return rect;
        }

        public static Rect TopHalf(this Rect rect)
        {
            rect = ReminderH(rect, 0.5f);
            return rect;
        }

        public static Rect BottomHalf(this Rect rect)
        {
            rect = SliceH(rect, 0.5f);
            return rect;
        }

        public static Rect Flex(this Rect rect, List<int> slices)
        {

            return rect;
        }

        private static void CheckPercent(float percent)
        {
            if (percent < 0 || percent > 1)
            {
                throw new Exception("Percent out of range");
            }
        }


    }
}
