using System;
using System.Reflection;
using System.Reflection.Emit;
using d768.DtoGenerator.Core.Infrastructure;
using LanguageExt;

namespace d768.DtoGenerator.Core.Emission
{
    public class EmittedType
    {
        private readonly EmittedTypeDefinition _typeDefinition;
        private readonly ModuleBuilder _moduleBuilder;

        public EmittedType(EmittedTypeDefinition typeDefinition, ModuleBuilder moduleBuilder)
        {
            _typeDefinition = typeDefinition;
            _moduleBuilder = moduleBuilder;
        }

        public Either<EmissionError, Type> CreateType()
        {
            return new Try<Type>(() =>
            {
                TypeBuilder tb = _moduleBuilder.DefineType(
                    _typeDefinition.Name,
                    TypeAttributes.Public
                    | TypeAttributes.Class);


                var ctor =
                    tb
                        .DefineDefaultConstructor(
                            MethodAttributes.Public
                            | MethodAttributes.SpecialName
                            | MethodAttributes.RTSpecialName);

                foreach (var property in _typeDefinition.Properties)
                {
                    CreateProperty(tb, property);
                }

                return tb.CreateType();
            })
                .ToEither()
                .MapLeft(x => new EmissionError(x));
        }

        private TypeBuilder CreateProperty(
            TypeBuilder typeBuilder, 
            EmittedTypeProperty property)
        {
            Type propertyType = Type.GetType(
                property.Type.FullName + ", " + property.Type.Module.Assembly.FullName); ;
        
            FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + property.Name, 
                propertyType, FieldAttributes.Private);  
  
            PropertyBuilder propertyBuilder = 
                typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, 
                    propertyType, null);  
            MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + property.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);  
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();  
  
            getIl.Emit(OpCodes.Ldarg_0);  
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);  
            getIl.Emit(OpCodes.Ret);  
  
            MethodBuilder setPropMthdBldr =typeBuilder.DefineMethod("set_" + property.Name,  
                MethodAttributes.Public |  
                MethodAttributes.SpecialName |  
                MethodAttributes.HideBySig,  
                null, new[] { propertyType });  
  
            ILGenerator setIl = setPropMthdBldr.GetILGenerator();  
            Label modifyProperty = setIl.DefineLabel();  
            Label exitSet = setIl.DefineLabel();  
  
            setIl.MarkLabel(modifyProperty);  
            setIl.Emit(OpCodes.Ldarg_0);  
            setIl.Emit(OpCodes.Ldarg_1);  
            setIl.Emit(OpCodes.Stfld, fieldBuilder);  
  
            setIl.Emit(OpCodes.Nop);  
            setIl.MarkLabel(exitSet);  
            setIl.Emit(OpCodes.Ret);  
  
            propertyBuilder.SetGetMethod(getPropMthdBldr);  
            propertyBuilder.SetSetMethod(setPropMthdBldr);

            return typeBuilder;
        }
    }
}