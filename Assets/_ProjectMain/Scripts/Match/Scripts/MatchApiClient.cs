using System;
using System.Collections;
using System.Text;
using EcosDelAzar.Match;
using UnityEngine;
using UnityEngine.Networking;

public class MatchApiClient : MonoBehaviour
{
    [Header("Configuración")]
    public string baseUrl = "http://localhost:8080/api/v1";

    // ── Public API ────────────────────────────────────────────

    /// <summary>POST /matches — Crea una nueva partida 1v1.</summary>
    public void CreateMatch(string playerOneId, string playerTwoId,
                            Action<CreateMatchResponse> onSuccess,
                            Action<ApiError> onError = null)
    {
        var body = new CreateMatchRequest
        {
            playerOneId = playerOneId,
            playerTwoId = playerTwoId
        };
        StartCoroutine(Post("matches", JsonUtility.ToJson(body), onSuccess, onError));
    }

    /// <summary>POST /matches/{matchId}/draw — El jugador roba una carta.</summary>
    public void DrawCard(string matchId, string playerId,
                         Action<DrawCardResponse> onSuccess,
                         Action<ApiError> onError = null)
    {
        var body = new DrawCardRequest { playerId = playerId };
        StartCoroutine(Post($"matches/{matchId}/draw", JsonUtility.ToJson(body), onSuccess, onError));
    }

    /// <summary>GET /matches/{matchId} — Estado actual de la partida.</summary>
    public void GetMatch(string matchId,
                         Action<MatchStateResponse> onSuccess,
                         Action<ApiError> onError = null)
    {
        StartCoroutine(Get($"matches/{matchId}", onSuccess, onError));
    }

    // ── Internals ─────────────────────────────────────────────

    private IEnumerator Post<T>(string endpoint, string jsonBody,
                                Action<T> onSuccess, Action<ApiError> onError)
    {
        string url = $"{baseUrl}/{endpoint}";
        using var request = new UnityWebRequest(url, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        HandleResponse(request, onSuccess, onError);
    }

    private IEnumerator Get<T>(string endpoint,
                               Action<T> onSuccess, Action<ApiError> onError)
    {
        string url = $"{baseUrl}/{endpoint}";
        using var request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Accept", "application/json");

        yield return request.SendWebRequest();

        HandleResponse(request, onSuccess, onError);
    }

    private void HandleResponse<T>(UnityWebRequest request,
                                   Action<T> onSuccess, Action<ApiError> onError)
    {
        // Conexión fallida — API no disponible
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.DataProcessingError)
        {
            var connError = new ApiError
            {
                status  = 0,
                error   = "CONNECTION_ERROR",
                message = "No se puede conectar con el servidor. Comprueba que la API está corriendo."
            };
            Debug.LogError($"[MatchApiClient] {connError.message}");
            onError?.Invoke(connError);
            return;
        }

        string body = request.downloadHandler?.text ?? string.Empty;
        Debug.Log($"[MatchApiClient] HTTP {request.responseCode} ← {request.url}\n{body}");

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                T data = JsonUtility.FromJson<T>(body);
                onSuccess?.Invoke(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MatchApiClient] Error parseando respuesta: {e.Message}");
                onError?.Invoke(new ApiError { status = 0, error = "PARSE_ERROR", message = e.Message });
            }
        }
        else
        {
            try
            {
                ApiError error = JsonUtility.FromJson<ApiError>(body);
                Debug.LogError($"[MatchApiClient] {error.status} {error.error} — {error.message}");
                onError?.Invoke(error);
            }
            catch
            {
                var error = new ApiError
                {
                    status  = (int)request.responseCode,
                    error   = "UNKNOWN",
                    message = request.error
                };
                Debug.LogError($"[MatchApiClient] {error.status} — {error.message}");
                onError?.Invoke(error);
            }
        }
    }
}
