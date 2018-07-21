using System;

namespace Groorine.Helpers
{
	public static class MathHelper
	{
		public static double EaseInOut(double time, double start, double end) =>
				(time /= 0.5) < 1
				? (end - start) * 0.5 * time * time * time + start
				: (end - start) * 0.5 * ((time -= 2) * time * time + 2) + start;
		public static double EaseIn(double time, double start, double end) => (end - start) * time * time * time + start;
		public static double EaseOut(double time, double start, double end) => (end - start) * (--time * time * time + 1) + start;
		public static double Linear(double time, double start, double end) => (end - start) * time + start;

		public static double Linear(double time, double timeStart, double timeEnd, double start, double end) => Linear((time - timeStart) / (timeEnd - timeStart), start, end);

		public static double ToRadian(double degree) => degree * 0.0055 * Math.PI;
	}
}
