using HarmonyLib;

using UnityEngine;

namespace Enhuddlement {
  public static class CodeMatcherExtensions {
    public static CodeMatcher SaveOperand(this CodeMatcher matcher, out object operand) {
      operand = matcher.Operand;
      return matcher;
    }
  }

  public static class ObjectExtensions {
    public static T Ref<T>(this T o) where T : Object {
      return o ? o : null;
    }
  }
}
