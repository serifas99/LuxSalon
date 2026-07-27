// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'frizer.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Frizer _$FrizerFromJson(Map<String, dynamic> json) => Frizer(
  id: (json['id'] as num?)?.toInt(),
  userId: (json['userId'] as num?)?.toInt(),
  imePrezime: json['imePrezime'] as String?,
  email: json['email'] as String?,
  profileImageBase64: json['profileImageBase64'] as String?,
  biografija: json['biografija'] as String?,
  specijalizacija: json['specijalizacija'] as String?,
  isActive: json['isActive'] as bool?,
  createdAt: json['createdAt'] == null
      ? null
      : DateTime.parse(json['createdAt'] as String),
  uslugaIds: (json['uslugaIds'] as List<dynamic>?)
      ?.map((e) => (e as num).toInt())
      .toList(),
);

Map<String, dynamic> _$FrizerToJson(Frizer instance) => <String, dynamic>{
  'id': instance.id,
  'userId': instance.userId,
  'imePrezime': instance.imePrezime,
  'email': instance.email,
  'profileImageBase64': instance.profileImageBase64,
  'biografija': instance.biografija,
  'specijalizacija': instance.specijalizacija,
  'isActive': instance.isActive,
  'createdAt': instance.createdAt?.toIso8601String(),
  'uslugaIds': instance.uslugaIds,
};
