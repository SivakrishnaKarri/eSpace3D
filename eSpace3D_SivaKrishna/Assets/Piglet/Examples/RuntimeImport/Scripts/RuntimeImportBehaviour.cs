using Piglet;
using UnityEngine;

/// <summary>
/// This MonoBehaviour provides a minimal example for using
/// Piglet to import glTF models at runtime.
/// </summary>
public class RuntimeImportBehaviour : MonoBehaviour
{
    /// <summary>
    /// The currently running glTF import task.
    /// </summary>
    private GltfImportTask _task;

    /// <summary>
    /// Root GameObject of the imported glTF model.
    /// </summary>
    private GameObject _model;

    public delegate void OnModelImportSuccess(GameObject g);
    public static event OnModelImportSuccess onModelImportSuccess;



    private bool importFlag = false;

    /// <summary>
    /// Unity callback that is invoked before the first frame.
    /// Create the glTF import task and set up callbacks for
    /// progress messages and successful completion.
    /// </summary>
    public void Import(string path)
    {

        /*_task = RuntimeGltfImporter.GetImportTask(
            "https://awesomesaucelabs.github.io/piglet-webgl-demo/StreamingAssets/piggleston.glb");*/
        _task = RuntimeGltfImporter.GetImportTask(path);
        _task.OnProgress = OnProgress;
        _task.OnCompleted = OnComplete;
        importFlag = true;
    }

    /// <summary>
    /// Callback that is invoked by the glTF import task
    /// after it has successfully completed.
    /// </summary>
    /// <param name="importedModel">
    /// the root GameObject of the imported glTF model
    /// </param>
    private void OnComplete(GameObject importedModel)
    {
        _model = importedModel;
        Debug.Log("Success!");
        importFlag = false;

        if(onModelImportSuccess != null)
            onModelImportSuccess(importedModel);
    }

    /// <summary>
    /// Callback that is invoked by the glTF import task
    /// to report intermediate progress.
    /// </summary>
    /// <param name="step">
    /// The current step of the glTF import process.  Each step imports
    /// a different type of glTF entity (e.g. textures, materials).
    /// </param>
    /// <param name="completed">
    /// The number of glTF entities (e.g. textures, materials) that have been
    /// successfully imported for the current import step.
    /// </param>
    /// <param name="total">
    /// The total number of glTF entities (e.g. textures, materials) that will
    /// be imported for the current import step.
    /// </param>
    private void OnProgress(ImportStep step, int completed, int total)
    {
        Debug.LogFormat("{0}: {1}/{2}", step, completed, total);
    }

    /// <summary>
    /// Unity callback that is invoked after every frame.
    /// Here we call MoveNext() to advance execution
    /// of the glTF import task. Once the model has been successfully
    /// imported, we auto-spin the model about the y-axis.
    /// </summary>
    void Update()
    {
        if (!importFlag)
            return;

        // advance execution of glTF import task
        _task.MoveNext();

        // spin model about y-axis
        //if (_model != null)
        //    _model.transform.Rotate(0, 1, 0);
    }
}
