// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'placanje.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Placanje _$PlacanjeFromJson(Map<String, dynamic> json) => Placanje(
  id: (json['id'] as num?)?.toInt(),
  terminId: (json['terminId'] as num?)?.toInt(),
  iznos: (json['iznos'] as num?)?.toDouble(),
  status: json['status'] as String?,
  paypalOrderId: json['paypalOrderId'] as String?,
  paypalTransactionId: json['paypalTransactionId'] as String?,
  createdAt: json['createdAt'] == null
      ? null
      : DateTime.parse(json['createdAt'] as String),
  datumPlacanja: json['datumPlacanja'] == null
      ? null
      : DateTime.parse(json['datumPlacanja'] as String),
  datumPovrata: json['datumPovrata'] == null
      ? null
      : DateTime.parse(json['datumPovrata'] as String),
);

Map<String, dynamic> _$PlacanjeToJson(Placanje instance) => <String, dynamic>{
  'id': instance.id,
  'terminId': instance.terminId,
  'iznos': instance.iznos,
  'status': instance.status,
  'paypalOrderId': instance.paypalOrderId,
  'paypalTransactionId': instance.paypalTransactionId,
  'createdAt': instance.createdAt?.toIso8601String(),
  'datumPlacanja': instance.datumPlacanja?.toIso8601String(),
  'datumPovrata': instance.datumPovrata?.toIso8601String(),
};

PlacanjeKreirajResponse _$PlacanjeKreirajResponseFromJson(
  Map<String, dynamic> json,
) => PlacanjeKreirajResponse(
  placanjeId: (json['placanjeId'] as num?)?.toInt(),
  paypalOrderId: json['paypalOrderId'] as String?,
  approvalUrl: json['approvalUrl'] as String?,
);

Map<String, dynamic> _$PlacanjeKreirajResponseToJson(
  PlacanjeKreirajResponse instance,
) => <String, dynamic>{
  'placanjeId': instance.placanjeId,
  'paypalOrderId': instance.paypalOrderId,
  'approvalUrl': instance.approvalUrl,
};
