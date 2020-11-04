using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SHW1.Middleware
{
    public class ReqResLogMid
    {
        private readonly RequestDelegate _next;

        public ReqResLogMid(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            //First, get the incoming request
            // Сначала получаем входящий запрос
            var request = await FormatRequest(httpContext.Request);

            //Copy a pointer to the original response body stream
            //Скопируйте указатель на исходный поток тела ответа
            var originalBodyStream = httpContext.Response.Body;

            //Create a new memory stream
            // Создаем новый поток памяти 
            using (var responseBody = new MemoryStream())
            {
                //and use that for the temporary response body
                // и используйте это для временного ответа body
                httpContext.Response.Body = responseBody;

                //Continue down the Middleware pipeline, eventually returning to this class
                //Продолжайте движение по конвейеру промежуточного ПО, в конечном итоге вернувшись к этому классу
                await _next(httpContext);

                //Format the response from the server
                //Отформатируйте ответ от сервера
                var response = await FormatResponse(httpContext.Response);

                //TODO: Save log to chosen datastore
                // ЗАДАЧА: Сохранить журнал в выбранное хранилище данных

                //Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
                // Копируем содержимое нового потока памяти (который содержит ответ) в исходный поток, который затем возвращается клиенту.
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
        private async Task<string> FormatRequest(HttpRequest request)
        {
            var body = request.Body;

            //This line allows us to set the reader for the request back at the beginning of its stream.
            // Эта строка позволяет нам вернуть читателя для запроса в начало его потока.
            request.EnableBuffering();

            //We now need to read the request stream.  First, we create a new byte[] with the same length as the request stream...
            // Теперь нам нужно прочитать поток запроса. Сначала мы создаем новый byte [] той же длины, что и поток запроса 
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];

            //Then we copy the entire request stream into the new buffer.
            // Затем мы копируем весь поток запроса в новый буфер.
            await request.Body.ReadAsync(buffer, 0, buffer.Length);

            //We convert the byte[] into a string using UTF8 encoding
            // Преобразуем byte [] в строку, используя кодировку UTF8 
            var bodyAsText = Encoding.UTF8.GetString(buffer);

            //..and finally, assign the read body back to the request body, which is allowed because of EnableRewind()
            //и, наконец, присваиваем чтение body обратно запроса body , что разрешено из-за EnableRewind ()
            request.Body = body;

            return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            //We need to read the response stream from the beginning
            // Нам нужно прочитать поток ответа с самого начала 
            response.Body.Seek(0, SeekOrigin.Begin);

            //...and copy it into a string
            // и скопируем в строку
            string text = await new StreamReader(response.Body).ReadToEndAsync();

            //We need to reset the reader for the response so that the client can read it.
            // Нам нужно сбросить считыватель ответа, чтобы клиент мог его прочитать.
            response.Body.Seek(0, SeekOrigin.Begin);

            //Return the string for the response, including the status code (e.g. 200)
            // Возвращаем строку ответа, включая код состояния (например, 200)
            return $"{response.StatusCode}: {text}";
        }
    }
}
