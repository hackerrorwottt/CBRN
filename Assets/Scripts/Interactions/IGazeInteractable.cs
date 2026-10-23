namespace CBRN.Interactions
{
    /// <summary>
    /// Interface implemented by any 3D object that can be interacted with
    /// using Cardboard gaze-dwell or screen tap.
    /// </summary>
    public interface IGazeInteractable
    {
        /// <summary>
        /// Called once when the player's gaze reticle first overlaps this object's collider.
        /// </summary>
        void OnGazeEnter();

        /// <summary>
        /// Called every frame the reticle remains over the object.
        /// </summary>
        /// <param name="normalizedProgress">Dwell timer progress from 0.0 (started) to 1.0 (completed).</param>
        void OnGazeStay(float normalizedProgress);

        /// <summary>
        /// Called once when the player looks away from this object.
        /// </summary>
        void OnGazeExit();

        /// <summary>
        /// Called when the dwell timer reaches 100% or when the trainee single-taps the screen/Cardboard button.
        /// </summary>
        void OnGazeTrigger();
    }
}
