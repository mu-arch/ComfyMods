using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// Provides simple tweening coroutines. Use them with StartCoroutine in your component.
/// Source: https://gist.github.com/FlaShG/5709a13fe5a145644ad9e7e62cffd8ef
/// </summary>
public static class CorouTweens {
  public enum UpdateType {
    Update, FixedUpdate
  }

  private static readonly YieldInstruction _waitForFixedUpdate = new WaitForFixedUpdate();

  #region Tweens
  public static AnimationCurve EaseInOut {
    get { return AnimationCurve.EaseInOut(0, 0, 1, 1); }
  }
  public static AnimationCurve EaseOut {
    get {
      return new AnimationCurve(new Keyframe(0, 0, 0, 2),
                                new Keyframe(1, 1, 0, 0));
    }
  }
  public static AnimationCurve BackOut {
    get {
      return new AnimationCurve(new Keyframe(0, 0, 0, 4),
                                new Keyframe(1, 1, 0, 0));
    }
  }
  public static AnimationCurve BounceOut {
    get {
      return new AnimationCurve(new Keyframe(0, 0, 0, 0),
                                new Keyframe(0.4f, 1, 5, -4),
                                new Keyframe(0.7f, 1, 4, -3),
                                new Keyframe(0.9f, 1, 3, -2),
                                new Keyframe(1, 1, 2, 0));
    }
  }
  #endregion

  #region Float
  /// <summary>
  /// Linearly interpolates from <paramref name="from"> to <paramref name="to"> for the given <paramref name="duration"> in seconds.
  /// Calls <paramref name="callback"> and passes the interpolated value to it in every step.
  /// <param name="from">The initial value to start from.</param>
  /// <param name="to">The target value to interpolate towards.</param>
  /// <param name="duration">The duration in seconds the interpolation is supposed to last.</param>
  /// <param name="callback">The callback called each step. The current interpolation result is passed to it.</param>
  /// <param name="updateType">Choose between interpolation in Update or FixedUpdate.</param>
  /// </summary>
  public static IEnumerator Lerp(float from,
                                 float to,
                                 float duration,
                                 Action<float> callback,
                                 UpdateType updateType = UpdateType.Update) {
    var waitInstruction = updateType == UpdateType.FixedUpdate ? _waitForFixedUpdate : null;

    for (var time = Time.deltaTime; time < duration; time += Time.deltaTime) {
      var t = time / duration;
      callback(Mathf.Lerp(from, to, t));
      yield return waitInstruction;
    }
    callback(to);
  }

  /// <summary>
  /// Linearly interpolates from <paramref name="from"> to <paramref name="to"> with the given <paramref name="speed">.
  /// Calls <paramref name="callback"> and passes the interpolated value to it in every step.
  /// <param name="from">The initial value to start from.</param>
  /// <param name="to">The target value to interpolate towards.</param>
  /// <param name="speed">The speed at which to move towards the target.</param>
  /// <param name="callback">The callback called each step. The current interpolation result is passed to it.</param>
  /// <param name="updateType">Choose between interpolation in Update or FixedUpdate.</param>
  /// </summary>
  public static IEnumerator LerpWithSpeed(float from,
                                          float to,
                                          float speed,
                                          Action<float> callback,
                                          UpdateType updateType = UpdateType.Update) {
    var waitInstruction = updateType == UpdateType.FixedUpdate ? _waitForFixedUpdate : null;

    var value = Mathf.MoveTowards(from, to, speed * Time.deltaTime);
    callback(value);
    while (value != to) {
      yield return waitInstruction;
      value = Mathf.MoveTowards(value, to, speed * Time.deltaTime);
      callback(value);
    }
    callback(to);
  }

  /// <summary>
  /// Interpolates from <paramref name="from"> to <paramref name="to"> for the given <paramref name="duration"> in seconds.
  /// Uses the given <paramref name="curve"> for tweening.
  /// Calls <paramref name="callback"> and passes the interpolated value to it in every step.
  /// <param name="from">The initial value to start from.</param>
  /// <param name="to">The target value to interpolate towards.</param>
  /// <param name="duration">The duration in seconds the interpolation is supposed to last.</param>
  /// <param name="curve">The curve describing the tween. Consider using the available ones from the CorouTween class.</param>
  /// <param name="callback">The callback called each step. The current interpolation result is passed to it.</param>
  /// <param name="updateType">Choose between interpolation in Update or FixedUpdate.</param>
  /// </summary>
  public static IEnumerator Interpolate(float from,
                                        float to,
                                        float duration,
                                        AnimationCurve curve,
                                        Action<float> callback,
                                        UpdateType updateType = UpdateType.Update) {
    var waitInstruction = updateType == UpdateType.FixedUpdate ? _waitForFixedUpdate : null;

    for (var time = Time.deltaTime; time < duration; time += Time.deltaTime) {
      var t = time / duration;
      callback(Mathf.Lerp(from, to, curve.Evaluate(t)));
      yield return waitInstruction;
    }
    callback(Mathf.Lerp(from, to, curve.Evaluate(1)));
  }
  #endregion

  #region Vector2
  /// <summary>
  /// Linearly interpolates from <paramref name="from"> to <paramref name="to"> for the given <paramref name="duration"> in seconds.
  /// Calls <paramref name="callback"> and passes the interpolated value to it in every step.
  /// <param name="from">The initial value to start from.</param>
  /// <param name="to">The target value to interpolate towards.</param>
  /// <param name="duration">The duration in seconds the interpolation is supposed to last.</param>
  /// <param name="callback">The callback called each step. The current interpolation result is passed to it.</param>
  /// <param name="updateType">Choose between interpolation in Update or FixedUpdate.</param>
  /// </summary>
  public static IEnumerator Lerp(Vector2 from,
                                 Vector2 to,
                                 float duration,
                                 Action<Vector2> callback,
                                 UpdateType updateType = UpdateType.Update) {
    var waitInstruction = updateType == UpdateType.FixedUpdate ? _waitForFixedUpdate : null;

    for (var time = Time.deltaTime; time < duration; time += Time.deltaTime) {
      var t = time / duration;
      callback(Vector2.Lerp(from, to, t));
      yield return waitInstruction;
    }
    callback(to);
  }

  /// <summary>
  /// Linearly interpolates from <paramref name="from"> to <paramref name="to"> with the given <paramref name="speed">.
  /// Calls <paramref name="callback"> and passes the interpolated value to it in every step.
  /// <param name="from">The initial value to start from.</param>
  /// <param name="to">The target value to interpolate towards.</param>
  /// <param name="speed">The speed at which to move towards the target.</param>
  /// <param name="callback">The callback called each step. The current interpolation result is passed to it.</param>
  /// <param name="updateType">Choose between interpolation in Update or FixedUpdate.</param>
  /// </summary>
  public static IEnumerator LerpWithSpeed(Vector2 from,
                                          Vector2 to,
                                          float speed,
                                          Action<Vector2> callback,
                                          UpdateType updateType = UpdateType.Update) {
    var waitInstruction = updateType == UpdateType.FixedUpdate ? _waitForFixedUpdate : null;

    var value = Vector2.MoveTowards(from, to, speed * Time.deltaTime);
    callback(value);
    while (value != to) {
      yield return waitInstruction;
      value = Vector2.MoveTowards(value, to, speed * Time.deltaTime);
      callback(value);
    }
    callback(to);
  }

  /// <summary>
  /// Interpolates from <paramref name="from"> to <paramref name="to"> for the given <paramref name="duration"> in seconds.
  /// Uses the given <paramref name="curve"> for tweening.
  /// Calls <paramref name="callback"> and passes the interpolated value to it in every step.
  /// <param name="from">The initial value to start from.</param>
  /// <param name="to">The target value to interpolate towards.</param>
  /// <param name="duration">The duration in seconds the interpolation is supposed to last.</param>
  /// <param name="curve">The curve describing the tween. Consider using the available ones from the CorouTween class.</param>
  /// <param name="callback">The callback called each step. The current interpolation result is passed to it.</param>
  /// <param name="updateType">Choose between interpolation in Update or FixedUpdate.</param>
  /// </summary>
  public static IEnumerator Interpolate(Vector2 from,
                                        Vector2 to,
                                        float duration,
                                        AnimationCurve curve,
                                        Action<Vector2> callback,
                                        UpdateType updateType = UpdateType.Update) {
    var waitInstruction = updateType == UpdateType.FixedUpdate ? _waitForFixedUpdate : null;

    for (var time = Time.deltaTime; time < duration; time += Time.deltaTime) {
      var t = time / duration;
      callback(Vector2.Lerp(from, to, curve.Evaluate(t)));
      yield return waitInstruction;
    }
    callback(Vector2.Lerp(from, to, curve.Evaluate(1)));
  }
  #endregion

  #region Vector3
  /// <summary>
  /// Linearly interpolates from <paramref name="from"> to <paramref name="to"> for the given <paramref name="duration"> in seconds.
  /// Calls <paramref name="callback"> and passes the interpolated value to it in every step.
  /// <param name="from">The initial value to start from.</param>
  /// <param name="to">The target value to interpolate towards.</param>
  /// <param name="duration">The duration in seconds the interpolation is supposed to last.</param>
  /// <param name="callback">The callback called each step. The current interpolation result is passed to it.</param>
  /// <param name="updateType">Choose between interpolation in Update or FixedUpdate.</param>
  /// </summary>
  public static IEnumerator Lerp(Vector3 from,
                                 Vector3 to,
                                 float duration,
                                 Action<Vector3> callback,
                                 UpdateType updateType = UpdateType.Update) {
    var waitInstruction = updateType == UpdateType.FixedUpdate ? _waitForFixedUpdate : null;

    for (var time = Time.deltaTime; time < duration; time += Time.deltaTime) {
      var t = time / duration;
      callback(Vector3.Lerp(from, to, t));
      yield return waitInstruction;
    }
    callback(to);
  }

  /// <summary>
  /// Linearly interpolates from <paramref name="from"> to <paramref name="to"> with the given <paramref name="speed">.
  /// Calls <paramref name="callback"> and passes the interpolated value to it in every step.
  /// <param name="from">The initial value to start from.</param>
  /// <param name="to">The target value to interpolate towards.</param>
  /// <param name="speed">The speed at which to move towards the target.</param>
  /// <param name="callback">The callback called each step. The current interpolation result is passed to it.</param>
  /// <param name="updateType">Choose between interpolation in Update or FixedUpdate.</param>
  /// </summary>
  public static IEnumerator LerpWithSpeed(Vector3 from,
                                          Vector3 to,
                                          float speed,
                                          Action<Vector3> callback,
                                          UpdateType updateType = UpdateType.Update) {
    var waitInstruction = updateType == UpdateType.FixedUpdate ? _waitForFixedUpdate : null;

    var value = Vector3.MoveTowards(from, to, speed * Time.deltaTime);
    callback(value);
    while (value != to) {
      yield return waitInstruction;
      value = Vector3.MoveTowards(value, to, speed * Time.deltaTime);
      callback(value);
    }
    callback(to);
  }

  /// <summary>
  /// Interpolates from <paramref name="from"> to <paramref name="to"> for the given <paramref name="duration"> in seconds.
  /// Uses the given <paramref name="curve"> for tweening.
  /// Calls <paramref name="callback"> and passes the interpolated value to it in every step.
  /// <param name="from">The initial value to start from.</param>
  /// <param name="to">The target value to interpolate towards.</param>
  /// <param name="duration">The duration in seconds the interpolation is supposed to last.</param>
  /// <param name="curve">The curve describing the tween. Consider using the available ones from the CorouTween class.</param>
  /// <param name="callback">The callback called each step. The current interpolation result is passed to it.</param>
  /// <param name="updateType">Choose between interpolation in Update or FixedUpdate.</param>
  /// </summary>
  public static IEnumerator Interpolate(Vector3 from,
                                        Vector3 to,
                                        float duration,
                                        AnimationCurve curve,
                                        Action<Vector3> callback,
                                        UpdateType updateType = UpdateType.Update) {
    var waitInstruction = updateType == UpdateType.FixedUpdate ? _waitForFixedUpdate : null;

    for (var time = Time.deltaTime; time < duration; time += Time.deltaTime) {
      var t = time / duration;
      callback(Vector3.Lerp(from, to, curve.Evaluate(t)));
      yield return waitInstruction;
    }
    callback(Vector3.Lerp(from, to, curve.Evaluate(1)));
  }
  #endregion

  #region Quaternion
  /// <summary>
  /// Spherically interpolates from <paramref name="from"> to <paramref name="to"> for the given <paramref name="duration"> in seconds.
  /// Calls <paramref name="callback"> and passes the interpolated value to it in every step.
  /// <param name="from">The initial value to start from.</param>
  /// <param name="to">The target value to interpolate towards.</param>
  /// <param name="duration">The duration in seconds the interpolation is supposed to last.</param>
  /// <param name="callback">The callback called each step. The current interpolation result is passed to it.</param>
  /// <param name="updateType">Choose between interpolation in Update or FixedUpdate.</param>
  /// </summary>
  public static IEnumerator Lerp(Quaternion from,
                                 Quaternion to,
                                 float duration,
                                 Action<Quaternion> callback,
                                 UpdateType updateType = UpdateType.Update) {
    var waitInstruction = updateType == UpdateType.FixedUpdate ? _waitForFixedUpdate : null;

    for (var time = Time.deltaTime; time < duration; time += Time.deltaTime) {
      var t = time / duration;
      callback(Quaternion.Slerp(from, to, t));
      yield return waitInstruction;
    }
    callback(to);
  }

  /// <summary>
  /// Spherically interpolates from <paramref name="from"> to <paramref name="to"> with the given <paramref name="speed">.
  /// Calls <paramref name="callback"> and passes the interpolated value to it in every step.
  /// <param name="from">The initial value to start from.</param>
  /// <param name="to">The target value to interpolate towards.</param>
  /// <param name="speed">The speed at which to move towards the target.</param>
  /// <param name="callback">The callback called each step. The current interpolation result is passed to it.</param>
  /// <param name="updateType">Choose between interpolation in Update or FixedUpdate.</param>
  /// </summary>
  public static IEnumerator LerpWithSpeed(Quaternion from,
                                          Quaternion to,
                                          float speed,
                                          Action<Quaternion> callback,
                                          UpdateType updateType = UpdateType.Update) {
    var waitInstruction = updateType == UpdateType.FixedUpdate ? _waitForFixedUpdate : null;

    var value = Quaternion.RotateTowards(from, to, speed * Time.deltaTime);
    callback(value);
    while (value != to) {
      yield return waitInstruction;
      value = Quaternion.RotateTowards(value, to, speed * Time.deltaTime);
      callback(value);
    }
    callback(to);
  }

  /// <summary>
  /// Interpolates from <paramref name="from"> to <paramref name="to"> for the given <paramref name="duration"> in seconds.
  /// Uses the given <paramref name="curve"> for tweening.
  /// Calls <paramref name="callback"> and passes the interpolated value to it in every step.
  /// <param name="from">The initial value to start from.</param>
  /// <param name="to">The target value to interpolate towards.</param>
  /// <param name="duration">The duration in seconds the interpolation is supposed to last.</param>
  /// <param name="curve">The curve describing the tween. Consider using the available ones from the CorouTween class.</param>
  /// <param name="callback">The callback called each step. The current interpolation result is passed to it.</param>
  /// <param name="updateType">Choose between interpolation in Update or FixedUpdate.</param>
  /// </summary>
  public static IEnumerator Interpolate(Quaternion from,
                                        Quaternion to,
                                        float duration,
                                        AnimationCurve curve,
                                        Action<Quaternion> callback,
                                        UpdateType updateType = UpdateType.Update) {
    var waitInstruction = updateType == UpdateType.FixedUpdate ? _waitForFixedUpdate : null;

    for (var time = Time.deltaTime; time < duration; time += Time.deltaTime) {
      var t = time / duration;
      callback(Quaternion.Slerp(from, to, curve.Evaluate(t)));
      yield return waitInstruction;
    }
    callback(Quaternion.Slerp(from, to, curve.Evaluate(1)));
  }
  #endregion
}