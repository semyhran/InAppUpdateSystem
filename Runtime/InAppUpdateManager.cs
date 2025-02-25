using UnityEngine;
#if UNITY_ANDROID
using Google.Play.Common;
using Google.Play.AppUpdate;
#endif
using System.Collections;

namespace InAppUpdateSystem
{
    public class InAppUpdateManager : MonoBehaviour
    {
#if UNITY_ANDROID
        // Manager for in-app update operations
        private AppUpdateManager appUpdateManager;

        // Singleton instance
        public static InAppUpdateManager Instance;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public IEnumerator CheckForUpdate(int remoteConfigValue = 0)
        {
            appUpdateManager = new AppUpdateManager();
            // Request update info from Play Store
            PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo();

            yield return appUpdateInfoOperation;


            if (appUpdateInfoOperation.IsSuccessful)
            {
                AppUpdateInfo appUpdateInfo = appUpdateInfoOperation.GetResult();

                // Check if an update is available
                if (appUpdateInfo.UpdateAvailability == UpdateAvailability.UpdateAvailable)
                {
                    int updatePriority = 0;

                    updatePriority = remoteConfigValue;

                    Debug.Log("Priority Number " + updatePriority);

                    // If the update has high priority, start an immediate update
                    if (updatePriority >= 4 && appUpdateInfo.IsUpdateTypeAllowed(AppUpdateOptions.ImmediateAppUpdateOptions()))
                    {
                        Debug.Log("Starting Immediate Update...");
                        yield return StartImmediateUpdate(appUpdateInfo);
                    }
                    // Otherwise, allow a flexible update
                    else if (updatePriority >= 1 && appUpdateInfo.IsUpdateTypeAllowed(AppUpdateOptions.FlexibleAppUpdateOptions()))
                    {
                        Debug.Log("Starting Flexible Update...");
                        StartCoroutine(StartFlexibleUpdate(appUpdateInfo));
                    }
                    else
                    {
                        Debug.Log("No suitable update type available.");
                    }
                }
                else
                {
                    Debug.Log("No update available.");
                }
            }
            else
            {
                Debug.LogError("Failed to get update info: " + appUpdateInfoOperation.Error);
            }
        }

        private IEnumerator StartFlexibleUpdate(AppUpdateInfo appUpdateInfo)
        {
            Debug.Log("Starting Flexible Update...");

            var appUpdateOptions = AppUpdateOptions.FlexibleAppUpdateOptions();
            var startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfo, appUpdateOptions);

            while (!startUpdateRequest.IsDone)
            {
                // For flexible flow,the user can continue to use the app while
                // the update downloads in the background. You can implement a
                // progress bar showing the download status during this time.
                yield return null;
            }

            if (startUpdateRequest.Error != AppUpdateErrorCode.NoError)
            {
                Debug.LogError("Failed to start update: " + startUpdateRequest.Error);
                //yield break;
            }

            Debug.Log("Flexible update started successfully!");

            while (true)
            {
                var updateInfoOperation = appUpdateManager.GetAppUpdateInfo();
                yield return updateInfoOperation;

                if (!updateInfoOperation.IsSuccessful)
                {
                    Debug.LogError("Failed to retrieve update status: " + updateInfoOperation.Error);
                    yield break;
                }

                var updateInfo = updateInfoOperation.GetResult();

                Debug.Log($"Update Status: {updateInfo.AppUpdateStatus}");

                if (updateInfo.AppUpdateStatus == AppUpdateStatus.Downloaded)
                {
                    Debug.Log("Flexible update downloaded. Completing update...");
                    yield return CompleteFlexibleUpdate();
                    yield break;
                }

                else if (updateInfo.AppUpdateStatus == AppUpdateStatus.Failed || updateInfo.AppUpdateStatus == AppUpdateStatus.Canceled || updateInfo.AppUpdateStatus == AppUpdateStatus.Unknown)
                {
                    Debug.LogError("Flexible update failed or was canceled.");
                    yield break;
                }

                yield return new WaitForSeconds(1f);
            }
        }

        private IEnumerator CompleteFlexibleUpdate()
        {
            var result = appUpdateManager.CompleteUpdate();
            yield return result;

            if (result.Error != AppUpdateErrorCode.NoError)
            {
                Debug.LogError("Flexible update installation failed: " + result.Error);
            }
        }

        private IEnumerator StartImmediateUpdate(AppUpdateInfo appUpdateInfo)
        {
            // Configure immediate update options
            var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
            var startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfo, appUpdateOptions);

            yield return startUpdateRequest;

            Debug.LogError("Immediate update failed or canceled: " + startUpdateRequest.Error);
            Application.Quit();
        }
#endif
    }
}