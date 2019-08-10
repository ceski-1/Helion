using System;
using Helion.Render.OpenGL.Context.Types;
using Helion.Util.Geometry;

namespace Helion.Render.OpenGL.Context
{
    /// <summary>
    /// A provider of GL functions.
    /// </summary>
    public interface IGLFunctions
    {
        void AttachShader(int programId, int shaderId);
        void BindAttribLocation(int programId, int attrIndex, string attrName);
        void BindBuffer(BufferType type, int bufferId);
        void BindTexture(TextureTargetType type, int textureId);
        void BindVertexArray(int vaoId);
        void BlendFunc(BlendingFactorType sourceFactor, BlendingFactorType destFactor);
        void BufferData<T>(BufferType bufferType, int totalBytes, T[] data, BufferUsageType usageType) where T : struct;
        void Clear(ClearType type);
        void ClearColor(float r, float g, float b, float a);
        void CompileShader(int shaderId);
        int CreateProgram();
        int CreateShader(ShaderComponentType type);
        void CullFace(CullFaceType type);
        void DebugMessageCallback(Action<DebugLevel, string> callback);
        void DeleteBuffer(int bufferId);
        void DeleteProgram(int programId);
        void DeleteShader(int shaderId);
        void DeleteTexture(int textureId);
        void DeleteVertexArray(int vaoId);
        void DetachShader(int programId, int shaderId);
        void DrawArrays(PrimitiveDrawType type, int startIndex, int count);
        void Enable(EnableType type);
        void EnableVertexAttribArray(int index);
        void FrontFace(FrontFaceType type);
        int GenBuffer();
        void GenerateMipmap(MipmapTargetType type);
        int GenTexture();
        int GenVertexArray();
        string GetActiveUniform(int mrogramId, int uniformIndex, out int size, out int typeEnum);
        ErrorType GetError();
        int GetInteger(GetIntegerType type);
        void GetProgram(int programId, GetProgramParameterType type, out int value);
        string GetProgramInfoLog(int programId);
        void GetShader(int shaderId, ShaderParameterType type, out int value);
        string GetShaderInfoLog(int shaderId);
        string GetString(GetStringType type);
        string GetString(GetStringType type, int index);
        long GetTextureHandleARB(int texture);
        int GetUniformLocation(int programId, string name);
        void LinkProgram(int mrogramId);
        void MakeTextureHandleNonResident(long handle);
        void MakeTextureHandleResidentARB(long handle);
        void ObjectLabel(ObjectLabelType type, int objectId, string name);
        void PolygonMode(PolygonFaceType faceType, PolygonModeType fillType);
        void ShaderSource(int shaderId, string sourceText);
        void TexParameter(TextureTargetType targetType, TextureParameterNameType paramType, int value);
        void TexStorage2D(TexStorageTargetType targetType, int mipmapLevels, TexStorageInternalType internalType, Dimension dimension);
        void TexSubImage2D(TextureTargetType targetType, int mipmapLevels, Vec2I position, Dimension dimension, PixelFormatType formatType, PixelDataType pixelType, IntPtr data);
        void Uniform1(int location, int value);
        void Uniform1(int location, float value);
        void UseProgram(int programId);
        void VertexAttribIPointer(int index, int size, VertexAttributeIntegralPointerType type, int stride, int offset);
        void VertexAttribPointer(int index, int byteLength, VertexAttributePointerType type, bool normalized, int stride, int offset);
        void Viewport(int x, int y, int width, int height);
    }
}