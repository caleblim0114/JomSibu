using System;
namespace JomSibu.Shared.SystemModels
{
	public class GenericResponse<T>
	{
        public bool IsSuccess { get; set; }
        public int? StatusCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}

