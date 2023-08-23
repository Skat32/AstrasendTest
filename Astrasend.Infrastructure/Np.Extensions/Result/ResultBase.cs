namespace Astrasend.Infrastructure.Np.Extensions.Result
{
    public abstract class ResultBase
    {
        /// <summary>
        /// <see cref="ErrorResponse"/>
        /// </summary>
        public ErrorResponse? ErrorResponse { get; protected set; }

        /// <summary>
        /// Is succeeded
        /// </summary>
        public bool IsSuccess => ErrorResponse == null;

        /// <summary>
        /// Is error reported when is not succeeded
        /// </summary>
        public bool WasErrorReported { get; set; } = false;
    }
}
